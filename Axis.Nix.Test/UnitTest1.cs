using System;
using Axis.Luna.Operation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Castle.DynamicProxy;

namespace Axis.Nix.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var gen = new ProxyGenerator();
            var iroot = new InterceptorRoot(new[] { new SampleInterceptor() });

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
            return "called";
        });
    }
}
