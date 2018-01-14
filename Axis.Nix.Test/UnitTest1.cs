using System;
using Axis.Luna.Operation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Castle.DynamicProxy;
using Axis.Proteus;
using System.Collections.Generic;

namespace Axis.Nix.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var gen = new ProxyGenerator();
            var registry = new InterceptorRegistry(typeof(SampleInterceptor), typeof(SampleInterceptor2));

            var iroot = new InterceptorRoot(new ActivatorServiceResolver(), registry);

            var proxy = gen.CreateInterfaceProxyWithoutTarget<ISampleInterface>(iroot);

            var start = DateTime.Now;
            var r = proxy.Method();
            Console.WriteLine($"call time: {DateTime.Now - start}");
            Assert.AreEqual(r, "called");

            start = DateTime.Now;
            r = proxy.Method2(5);
            Console.WriteLine($"call time: {DateTime.Now - start}");
            Assert.AreEqual(r, "called");
        }

        [TestMethod]
        public void TestMethod2()
        {
            var ex = new Exception("the exception");
            var op = InterceptorRoot.CreateFailedOperation(typeof(IOperation), ex);
            op = InterceptorRoot.CreateFailedOperation(typeof(IOperation<string>), ex);
            op = InterceptorRoot.CreateFailedOperation(typeof(ResolvedOperation), ex);
            op = InterceptorRoot.CreateFailedOperation(typeof(ResolvedOperation<DateTimeOffset>), ex);
        }
    }

    public interface ISampleInterface
    {
        string Method();
        string Method2(int df);
    }


    public class SampleInterceptor : IProxyInterceptor
    {
        public IOperation<object> Intercept(InvocationContext context)
        => LazyOp.Try(() =>
        {
            if (context.Method.Name == "Method2") Assert.AreEqual(context.Arguments[0], 5);
            var value = context.Next?
                .Invoke()
                .Resolve();
            return "called";
        });
    }

    public class SampleInterceptor2: SampleInterceptor
    {
    }

    public class ActivatorServiceResolver : IServiceResolver
    {
        public void Dispose()
        {
        }

        public Service Resolve<Service>()
        {
            return Activator.CreateInstance<Service>();
        }

        public object Resolve(Type serviceType)
        {
            return Activator.CreateInstance(serviceType);
        }

        public IEnumerable<Service> ResolveAll<Service>()
        {
            return new[] { Activator.CreateInstance<Service>() };
        }

        public IEnumerable<object> ResolveAll(Type serviceType)
        {
            return new[] { Activator.CreateInstance(serviceType) };
        }
    }
}
