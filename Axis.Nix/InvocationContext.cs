using Axis.Luna.Extensions;
using Axis.Luna.Operation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Axis.Nix
{
    #region Old Code
    //public class InvocationContext
    //{
    //    private IEnumerator<IProxyInterceptor> _proxies;
    //    private IProxyInterceptor _currentInterceptor;

    //    public InvocationContext(MethodInfo method, object proxy, object[] arguments = null, Type[] genericArgs = null, params IProxyInterceptor[] proxies)
    //    {
    //        Arguments = arguments ?? new object[0];
    //        GenericArguments = genericArgs ?? new Type[0];
    //        Proxy = proxy.ThrowIfNull("Invalid proxy");
    //        Method = method.ThrowIfNull("Invalid invocation method");

    //        _proxies = proxies
    //            .Cast<IProxyInterceptor>()
    //            .GetEnumerator();
    //        _currentInterceptor = _proxies.MoveNext() ? _proxies.Current : null;
    //        InitNext();
    //    }

    //    private InvocationContext(InvocationContext parent)
    //    {
    //        Method = parent.Method;
    //        Proxy = parent.Proxy;
    //        Arguments = parent.Arguments?.Clone().Cast<object[]>();
    //        GenericArguments = parent.GenericArguments?.Clone().Cast<Type[]>();

    //        _proxies = parent._proxies;
    //        _currentInterceptor = _proxies.MoveNext() ? _proxies.Current : null;
    //        InitNext();
    //    }

    //    private void InitNext()
    //    {
    //        if(_currentInterceptor != null)
    //        {
    //            Next = () => LazyOp.Try(() =>
    //            {
    //                var child = new InvocationContext(this);
    //                return _currentInterceptor
    //                    .Intercept(child)
    //                    .Resolve();
    //            });
    //        }
    //    }

    //    public object[] Arguments { get; private set; }

    //    public Type[] GenericArguments { get; private set; }

    //    public MethodInfo Method { get; private set; }

    //    public object Proxy { get; private set; }


    //    public Func<IOperation<object>> Next { get; private set; }
    //}
    #endregion

    public class InvocationContext
    {
        private IEnumerator<IProxyInterceptor> _proxies;
        private IProxyInterceptor _currentInterceptor;
        private Func<object> _targetFactory;
        private object _target;

        public object[] Arguments { get; private set; }

        public Type[] GenericArguments { get; private set; }

        public MethodInfo Method { get; private set; }

        public object Target => _target ?? (_target = _targetFactory());

        public object Proxy { get; private set; }

        public Func<IOperation<object>> Next { get; private set; }


        public InvocationContext(MethodInfo method, object proxy, Func<object> targetFactory, object[] arguments = null, Type[] genericArgs = null, params IProxyInterceptor[] proxies)
        {
            Arguments = arguments ?? new object[0];
            GenericArguments = genericArgs ?? new Type[0];
            _targetFactory = targetFactory;
            Proxy = proxy.ThrowIfNull("Invalid proxy");
            Method = method.ThrowIfNull("Invalid invocation method");

            _proxies = proxies
                .Cast<IProxyInterceptor>()
                .GetEnumerator();
            _currentInterceptor = _proxies.MoveNext() ? _proxies.Current : null;
            InitNext();
        }

        private InvocationContext(InvocationContext parent)
        {
            Method = parent.Method;
            Proxy = parent.Proxy;
            Arguments = parent.Arguments?.Clone().Cast<object[]>();
            GenericArguments = parent.GenericArguments?.Clone().Cast<Type[]>();

            _proxies = parent._proxies;
            _currentInterceptor = _proxies.MoveNext() ? _proxies.Current : null;
            _target = parent._target;
            _targetFactory = parent._targetFactory;

            InitNext();
        }

        private void InitNext()
        {
            if (_currentInterceptor != null)
            {
                Next = () => LazyOp.Try(() =>
                {
                    var child = new InvocationContext(this);
                    return _currentInterceptor
                        .Intercept(child)
                        .Resolve();
                });
            }
        }
    }
}
