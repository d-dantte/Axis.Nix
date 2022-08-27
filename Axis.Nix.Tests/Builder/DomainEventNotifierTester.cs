using Axis.Luna.Extensions;
using Axis.Luna.Operation;
using Axis.Nix.Builder;
using Axis.Nix.Configuration;
using Axis.Nix.Exceptions;
using Axis.Nix.Handler;
using Axis.Nix.Tests.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Axis.Nix.Tests.Builder
{
    [TestClass]
    public class DomainEventNotifierTester
    {
        [TestMethod]
        public void Constructor_WithValidParams_ShouldCreateNotifier()
        {

            // setup
            var options = new Options(
                new AsyncOptions(
                    cancellationToken: null,
                    taskCreationOptions: TaskCreationOptions.None));

            var handlers = new[]
            {
                (typeof(Event1), new List<IHandler>
                {
                    new EventHandler1()
                })
            };

            // test
            var notifier = new DomainEventNotifier(options, handlers);

            // assert
            Assert.IsNotNull(notifier);
        }

        [TestMethod]
        public void Constructor_WithdisabledAsyncBehavior_ShouldCreateNotifier()
        {

            // setup
            var options = new Options(null);

            var handlers = new[]
            {
                (typeof(Event1), new List<IHandler>
                {
                    new EventHandler1()
                })
            };

            // test
            var notifier = new DomainEventNotifier(options, handlers);

            // assert
            Assert.IsNotNull(notifier);
        }

        [TestMethod]
        public async Task Notify_WithSyncNotification_ShouldCallHandlerNotify()
        {
            // setup
            var mockHandler = new Mock<IEventHandler<Event1>>();
            mockHandler
                .Setup(h => h.HandleEvent(It.IsAny<Event1>()))
                .Returns(Operation.FromVoid())
                .Verifiable();

            var notifier = NotifierBuilder
                .Create()
                .Configure(new Options(null))
                .WithHandler(mockHandler.Object)
                .Build();

            // test
            var op = notifier.Notify(new Event1());
            await op;
            await notifier.Notify(new Event1());

            // verify
            Assert.IsNotNull(op);
            Assert.AreEqual(true, op.Succeeded);
            mockHandler.Verify();
        }

        [TestMethod]
        public async Task Notify_WithAsyncNotification_ShouldCallHandlerNotify()
        {
            // setup
            var mockHandler = new Mock<IEventHandler<Event1>>();
            mockHandler
                .Setup(h => h.HandleEvent(It.IsAny<Event1>()))
                .Returns(Operation.FromVoid())
                .Verifiable();

            var notifier = NotifierBuilder
                .Create()
                .Configure(
                    new Options(
                        new AsyncOptions(
                            cancellationToken: null,
                            taskCreationOptions: TaskCreationOptions.None)))
                .WithHandler(mockHandler.Object)
                .Build();

            // test
            var op = notifier.Notify(new Event1());
            Thread.Sleep(500);
            await op;

            // verify
            Assert.IsNotNull(op);
            Assert.AreEqual(true, op.Succeeded);
            mockHandler.Verify();
        }

        [TestMethod]
        public async Task Notify_WithUnregisteredEvent_ShouldThrowException()
        {
            // setup
            var mockHandler = new Mock<IEventHandler<Event1>>();

            var notifier = NotifierBuilder
                .Create()
                .Configure(new Options(null))
                .WithHandler(mockHandler.Object)
                .Build();

            // test
            var op = notifier.Notify(new Event2());

            // verify
            Assert.AreEqual(false, op.Succeeded);
            await Assert.ThrowsExceptionAsync<HandlerNotFoundException>(async () => await op);
        }

        [TestMethod]
        public async Task Notify_WithNullEvent_ShouldThrowArgumentNullException()
        {
            // setup
            var mockHandler = new Mock<IEventHandler<Event1>>();

            var notifier = NotifierBuilder
                .Create()
                .Configure(new Options(null))
                .WithHandler(mockHandler.Object)
                .Build();

            // test
            var op = notifier.Notify<Event1>(null);

            // verify
            Assert.AreEqual(false, op.Succeeded);
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await op);
        }

        [TestMethod]
        public async Task Notify_WithNoRegisteredEvent_ShouldNoOp()
        {
            // setup
            var notifier = new DomainEventNotifier(
                new Options(null),
                (typeof(Event1), new List<IHandler>()),
                (typeof(Event2), new List<IHandler>()));

            // test
            var op = notifier.Notify(new Event1());
            await op;

            // verify
            Assert.IsNotNull(op);
            Assert.AreEqual(true, op.Succeeded);
        }

        [TestMethod]
        public async Task Notify_WithAsyncNotification_AndErrorConsumers_ShouldConsumeErrors()
        {
            // setup
            var @event = new Event1();

            var mockHandler = new Mock<IEventHandler<Event1>>();
            mockHandler
                .Setup(h => h.HandleEvent(It.IsAny<Event1>()))
                .Returns(Operation.Fail(new Exception("raised")))
                .Verifiable();

            var mockErrorConsumer = new Mock<IErrorConsumer>();
            mockErrorConsumer
                .Setup(c => c.ConsumeErrors(It.IsAny<HandlerException[]>()))
                .Verifiable();

            var notifier = NotifierBuilder
                .Create()
                .Configure(
                    new Options(
                        new AsyncOptions(
                            cancellationToken: null,
                            taskCreationOptions: TaskCreationOptions.None,
                            mockErrorConsumer.Object.ConsumeErrors)))
                .WithHandler(mockHandler.Object)
                .Build();

            // test
            var op = notifier.Notify(@event);
            Thread.Sleep(500);
            await op;

            // verify
            Assert.IsNotNull(op);
            Assert.AreEqual(true, op.Succeeded);
            mockHandler.Verify();
        }

        [TestMethod]
        public async Task Notify_WithAsyncNotification_AndNoErrorConsumers_AndRegisteredSink_ShouldConsumeErrors()
        {
            // setup
            var @event = new Event1();

            var mockHandler = new Mock<IEventHandler<Event1>>();
            mockHandler
                .Setup(h => h.HandleEvent(It.IsAny<Event1>()))
                .Returns(Operation.Fail(new Exception("raised")))
                .Verifiable();

            Task handlerTask = null;
            var notifier = NotifierBuilder
                .Create()
                .Configure(
                    new Options(
                        new AsyncOptions(
                            taskSink: t => { handlerTask = t; }, // no-op
                            cancellationToken: null,
                            taskCreationOptions: TaskCreationOptions.None)))
                .WithHandler(mockHandler.Object)
                .Build();

            // test
            await notifier.Notify(new Event1());
            await Task.Delay(500);

            // verify
            mockHandler.Verify();
            Assert.AreEqual(TaskStatus.Faulted, handlerTask.Status);
        }


        public interface IErrorConsumer
        {
            void ConsumeErrors(HandlerException[] exception);

            void ConsumeAndThrowErrors(HandlerException[] exception);

            void Sink(Task task);
        }

    }
}
