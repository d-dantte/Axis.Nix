using Axis.Luna.Extensions;
using Castle.DynamicProxy;
using System.Collections.Generic;

namespace Axis.Nix
{
    public class InterceptorRoot: IInterceptor
    {
        private List<IProxyInterceptor> _interceptors = new List<IProxyInterceptor>();

        public InterceptorRoot(params IProxyInterceptor[] @params)
        {
            _interceptors.AddRange(@params);
        }

        public InterceptorRoot AddInterceptor(IProxyInterceptor interceptor)
        {
            _interceptors.Add(interceptor.ThrowIfNull("Invalid interceptor"));
            return this;
        }

        public IProxyInterceptor[] Interceptors() => _interceptors.ToArray();



        public void Intercept(IInvocation invocation)
        {
            var context = new InvocationContext(invocation.Method,
                                                invocation.Proxy,
                                                invocation.Arguments,
                                                invocation.GenericArguments,
                                                Interceptors());

            context
                .Next()
                .Then(@return => invocation.ReturnValue = @return)
                .Resolve();
        }
    }
}
