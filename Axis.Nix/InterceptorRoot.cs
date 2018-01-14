using Axis.Luna.Extensions;
using Castle.DynamicProxy;
using System.Collections.Generic;
using System;
using Axis.Proteus;
using System.Linq;
using Axis.Luna.Operation;
using Axis.Rhea;

namespace Axis.Nix
{
    public class InterceptorRoot: IInterceptor
    {
        private List<IProxyInterceptor> _interceptors = new List<IProxyInterceptor>();
        private Func<object> _targetProvider = null;

        public InterceptorRoot(IServiceResolver resolver, InterceptorRegistry registry)
        {
            _interceptors.AddRange(registry.Interceptors()
                                           .Select(_type => resolver.Resolve(_type).Cast<IProxyInterceptor>()));
        }
        public InterceptorRoot(IServiceResolver resolver, Func<object> targetProvider, InterceptorRegistry registry)
        {
            _targetProvider = targetProvider;
            _interceptors.AddRange(registry.Interceptors()
                                           .Select(_type => resolver.Resolve(_type).Cast<IProxyInterceptor>()));
        }



        public void Intercept(IInvocation invocation)
        {
            var context = new InvocationContext(invocation.Method,
                                                invocation.Proxy,
                                                _targetProvider,
                                                invocation.Arguments,
                                                invocation.GenericArguments,
                                                _interceptors.ToArray());

            context
                .Next()
                .ContinueWith(_op => 
                {
                    if(_op.Succeeded == true)
                        invocation.ReturnValue = _op.Resolve();

                    //if the interception failed, and the invocation is an operation (returns an operation),
                    //wrap the error of the invocation into a failed operation and return that
                    else if (InvocationReturnsOperation(invocation))
                        invocation.ReturnValue = CreateFailedOperation(invocation.Method.ReturnType, _op.GetException());

                    //else throw out the exception let castle handle it
                    else throw new Exception("See Inner Exception", _op.GetException());
                })
                .Resolve();
        }

        private bool InvocationReturnsOperation(IInvocation invocation)
        {
            var returnType = invocation.Method.ReturnType;
            if (returnType.IsClass)
            {
                if (returnType.Implements(typeof(IOperation))) return true;
                else if (returnType.ImplementsGenericInterface(typeof(IOperation<>))) return true;
            }
            else if (returnType.IsInterface)
            {
                if (returnType == typeof(IOperation)) return true;
                else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(IOperation<>)) return true;
            }

            return false;
        }

        public static object CreateFailedOperation(Type operationType, Exception ex)
        {
            if(operationType.IsInterface)
            {
                var top = typeof(IOperation);
                if (operationType == top || operationType.Implements(top))
                    return ResolvedOp.Fail(ex);

                else //if(operationType.ImplementsGenericInterface(typeof(IOperation<>)))
                {
                    var genericParam = operationType.GetGenericArguments()[0];
                    var failedMethod = typeof(ResolvedOp)
                        .GetMethods()
                        .Where(_m => _m.IsStatic)
                        .Where(_m => _m.Name == nameof(ResolvedOp.Fail))
                        .Where(_m => _m.IsGenericMethod)
                        .FirstOrDefault()
                        .MakeGenericMethod(genericParam);

                    return failedMethod.Call(ex);
                }
            }
            else //if(operationType.IsClass)
            {
                if (operationType == typeof(AsyncOperation))
                    return AsyncOp.Fail(ex);

                else if (operationType == typeof(ResolvedOperation))
                    return ResolvedOp.Fail(ex);

                else if (operationType == typeof(LazyOperation))
                    return LazyOp.Fail(ex);

                else // if(operationType.ImplementsGenericInterface(typeof(IOperation<>)))
                {
                    var typeDefinition = operationType.GetGenericTypeDefinition();
                    var genericParam = operationType.GetGenericArguments()[0];
                    var helper = typeDefinition == typeof(LazyOperation<>) ? typeof(LazyOp) :
                                 typeDefinition == typeof(ResolvedOperation<>) ? typeof(ResolvedOp) :
                                 typeof(AsyncOp);
                    var failedMethod = helper
                        .GetMethods()
                        .Where(_m => _m.IsStatic)
                        .Where(_m => _m.Name == nameof(ResolvedOp.Fail))
                        .Where(_m => _m.IsGenericMethod)
                        .FirstOrDefault()
                        .MakeGenericMethod(genericParam);

                    return failedMethod.Call(ex);
                }
            }
        }
    }
}
