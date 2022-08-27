using Axis.Nix.Builder;
using Axis.Nix.Configuration;
using Axis.Nix.Handler;
using Axis.Nix.Tests.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Axis.Nix.Tests.Builder
{
    [TestClass]
    public class NotifierBuilderTest
    {

        [TestMethod]
        public void Configure_WithValidOptions_ShouldCreateValidBuilder()
        {
            // setup
            var builder = NotifierBuilder.Create();
            var options = new Options(
                new AsyncOptions(
                    taskCreationOptions: TaskCreationOptions.None,
                    errorConsumers: hex => Console.WriteLine(hex)));

            // act
            _ = builder.Configure(options);

            // assert
            Assert.AreEqual(options, builder.BuilderOptions());
        }

        #region NotifierBuilder WithHandler<THandler>()
        [TestMethod]
        public void WithHandler1_WithValidHandler_ShouldRegisterHandler()
        {
            // setup
            var builder = NotifierBuilder
                .Create()
                .Configure(new Options(
                new AsyncOptions(
                    taskCreationOptions: TaskCreationOptions.None,
                    errorConsumers: hex => Console.WriteLine(hex))));

            // act
            _ = builder.WithHandler<EventHandler1>();

            // assert
            Assert.IsTrue(builder.HandlerMaps().ContainsKey(typeof(Event1)));
            Assert.IsInstanceOfType(builder.HandlerMaps()[typeof(Event1)][0], typeof(EventHandler1));
        }

        [TestMethod]
        public void WithHandler1_WithInvalidHandler_ShouldThrowException()
        {
            // setup
            var builder = NotifierBuilder
                .Create()
                .Configure(new Options(
                new AsyncOptions(
                    taskCreationOptions: TaskCreationOptions.None,
                    errorConsumers: hex => Console.WriteLine(hex))));

            // act/assert
            Assert.ThrowsException<Exception>(() => builder.WithHandler<FauxIHandler>());
        }
        #endregion


        #region NotifierBuilder WithHandler<TEvent>(IEventHandler<TEvent> handlerInstance)
        [TestMethod]
        public void WithHandler2_WithValidInstance_ShouldRegisterHandler()
        {
            // setup
            var builder = NotifierBuilder
                .Create()
                .Configure(new Options(
                new AsyncOptions(
                    taskCreationOptions: TaskCreationOptions.None,
                    errorConsumers: hex => Console.WriteLine(hex))));

            // act
            _ = builder.WithHandler(new EventHandler1());

            // assert
            Assert.IsTrue(builder.HandlerMaps().ContainsKey(typeof(Event1)));
            Assert.IsInstanceOfType(builder.HandlerMaps()[typeof(Event1)][0], typeof(EventHandler1));
        }

        [TestMethod]
        public void WithHandler2_WithNullInstance_ShouldThrowException()
        {
            // setup
            var builder = NotifierBuilder
                .Create()
                .Configure(new Options(
                new AsyncOptions(
                    taskCreationOptions: TaskCreationOptions.None,
                    errorConsumers: hex => Console.WriteLine(hex))));

            // act/assert
            Assert.ThrowsException<ArgumentNullException>(() => builder.WithHandler<Event1>(null));
        }
        #endregion


        #region NotifierBuilder WithHandler<TEvent>(IDeferredEventHandler<TEvent> handlerInstance)
        [TestMethod]
        public void WithHandler3_WithValidInstance_ShouldRegisterHandler()
        {
            // setup
            var builder = NotifierBuilder
                .Create()
                .Configure(new Options(
                new AsyncOptions(
                    taskCreationOptions: TaskCreationOptions.None,
                    errorConsumers: hex => Console.WriteLine(hex))));

            // act
            _ = builder.WithHandler(new EventHandler1());

            // assert
            Assert.IsTrue(builder.HandlerMaps().ContainsKey(typeof(Event1)));
            Assert.IsInstanceOfType(builder.HandlerMaps()[typeof(Event1)][0], typeof(EventHandler1));
        }

        [TestMethod]
        public void WithHandler3_WithNullInstance_ShouldThrowException()
        {
            // setup
            var builder = NotifierBuilder
                .Create()
                .Configure(new Options(
                new AsyncOptions(
                    taskCreationOptions: TaskCreationOptions.None,
                    errorConsumers: hex => Console.WriteLine(hex))));

            // act/assert
            Assert.ThrowsException<ArgumentNullException>(() => builder.WithHandler((EventHandler1)null));
        }
        #endregion


        #region NotifierBuilder WithHandler<TEvent, TResult>(IDeferredEventHandler<TEvent, TResult> handlerInstance)
        [TestMethod]
        public void WithHandler4_WithValidInstance_ShouldRegisterHandler()
        {
            // setup
            var builder = NotifierBuilder
                .Create()
                .Configure(new Options(
                new AsyncOptions(
                    taskCreationOptions: TaskCreationOptions.None,
                    errorConsumers: hex => Console.WriteLine(hex))));

            // act
            _ = builder.WithHandler(new EventHandler1());

            // assert
            Assert.IsTrue(builder.HandlerMaps().ContainsKey(typeof(Event1)));
            Assert.IsInstanceOfType(builder.HandlerMaps()[typeof(Event1)][0], typeof(EventHandler1));
        }

        [TestMethod]
        public void WithHandler4_WithNullInstance_ShouldThrowException()
        {
            // setup
            var builder = NotifierBuilder
                .Create()
                .Configure(new Options(
                new AsyncOptions(
                    taskCreationOptions: TaskCreationOptions.None,
                    errorConsumers: hex => Console.WriteLine(hex))));

            // act/assert
            Assert.ThrowsException<ArgumentNullException>(
                () => builder.WithHandler((EventHandler1)null));
        }
        #endregion

        #region NotifierBuilder Build()

        #endregion

    }
}
