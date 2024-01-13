using Axis.Luna.Extensions;
using Axis.Nix.Configuration;
using Axis.Nix.Event;
using Axis.Nix.Exceptions;
using Axis.Nix.Handler;
using Axis.Nix.Tests.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Axis.Nix.Tests.Unit
{
    [TestClass]
    public class DomainEventNotifierTester
    {

        [TestMethod]
        public void Constructor_ShouldReturnValidInstance()
        {
            var instance = new DomainEventNotifier(
                default,
                (typeof(Event1Data), new List<Lazy<IHandler>> { new Lazy<IHandler>(new Event1Handler()) }));

            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void Constructor_WithInvalidArgs_ShouldThrowException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new DomainEventNotifier(
                default,
                (IEnumerable<(Type eventType, List<Lazy<IHandler>> handlers)>)null));

            Assert.ThrowsException<ArgumentNullException>(() => new DomainEventNotifier(
                default,
                (typeof(Event1Data), null)));

            Assert.ThrowsException<ArgumentException>(() => new DomainEventNotifier(
                default,
                (typeof(Event1Data), new List<Lazy<IHandler>> { null })));
        }

        [TestMethod]
        public void RegisteredEvents_ReturnsArrayOfRegisteredEventTypes()
        {
            var instance = new DomainEventNotifier(
                default,
                (typeof(Event1Data), new List<Lazy<IHandler>> { new Lazy<IHandler>(new Event1Handler()) }));

            var registeredTypes = instance.RegisteredEvents();
            Assert.IsNotNull(registeredTypes);
            Assert.AreEqual(1, registeredTypes.Length);
            Assert.AreEqual(typeof(Event1Data), registeredTypes[0]);
        }

        [TestMethod]
        public async Task Notify_WithNoAsyncBehavior_ShouldHandleEvent()
        {
            var handler = new Event1Handler();
            var notifier = new DomainEventNotifier(
                default,
                (typeof(Event1Data), new List<Lazy<IHandler>> { new Lazy<IHandler>(handler) }));
            var event1 = new DomainEvent<Event1Data>("event-1", new Event1Data());

            await notifier.Notify(event1);
            Assert.AreEqual(1, handler.HandleCount);
        }

        [TestMethod]
        public async Task Notify_WithAsyncBehavior_ShouldHandleEvent()
        {
            var handler = new Event1Handler();
            var notifier = new DomainEventNotifier(
                new Options(
                    new AsyncOptions(
                        cancellationToken: null,
                        taskCreationOptions: TaskCreationOptions.LongRunning)),
                (typeof(Event1Data), new List<Lazy<IHandler>> { new Lazy<IHandler>(handler) }));
            var event1 = new DomainEvent<Event1Data>("event-1", new Event1Data());

            await notifier.Notify(event1);

            while (handler.HandleCount == 0)
                await Task.Delay(5);

            Assert.AreEqual(1, handler.HandleCount);
        }

        [TestMethod]
        public async Task Notify_WithInvalidArgs_ShouldThrowException()
        {
            var notifier = new DomainEventNotifier(
                default,
                (typeof(Event1Data), new List<Lazy<IHandler>> { new Lazy<IHandler>(new Event1Handler()) }));
            var event2 = new DomainEvent<Event2Data>("event-2", new Event2Data());

            await Assert.ThrowsExceptionAsync<HandlerNotFoundException>(async () => await notifier.Notify(event2));
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await notifier.Notify<Event1Data>(default));
        }

        [TestMethod]
        public async Task Notify_WithNoHandlerFound_ShouldNoOp()
        {
            var notifier = new DomainEventNotifier(
                default,
                (typeof(Event1Data), new List<Lazy<IHandler>> { new Lazy<IHandler>(new Event1PrefixHandler("abcd")) }));
            var event1 = new DomainEvent<Event1Data>("event-1", new Event1Data());

            await notifier.Notify(event1);
        }

        [TestMethod]
        public async Task Notify_WithFaultingHandler_ShouldThrowFault()
        {
            var fault = new InvalidProgramException("something");
            var notifier = new DomainEventNotifier(
                default,
                (typeof(Event1Data), new List<Lazy<IHandler>> { new Lazy<IHandler>(new FaultingEvent1Handler(fault)) }));
            var event1 = new DomainEvent<Event1Data>("event-1", new Event1Data());

            var op = notifier.Notify(event1);
            var exception = await Assert.ThrowsExceptionAsync<InvalidProgramException>(async () => await op);
            Assert.AreEqual(fault, exception);
        }

        [TestMethod]
        public async Task Notify_WithNoAsyncBehavior_ShouldNotifySynchroniously()
        {
            var delay = TimeSpan.FromSeconds(0.5);
            var notifier = new DomainEventNotifier(
                default,
                (typeof(Event1Data), new List<Lazy<IHandler>> { new Lazy<IHandler>(new DelayedEvent1Handler(delay)) }));
            var event1 = new DomainEvent<Event1Data>("event-1", new Event1Data());

            var sw = Stopwatch.StartNew();
            await notifier.Notify(event1);
            sw.Stop();

            Assert.IsTrue(sw.Elapsed >= delay);
        }

        [TestMethod]
        public async Task Notify_WithAsyncBehavior_ShouldNotifyAsynchroniously()
        {
            var delay = TimeSpan.FromSeconds(0.5);
            CancellationTokenSource cts = new();
            var notifier = new DomainEventNotifier(
                new Options(
                    new AsyncOptions(
                        cancellationToken: cts.Token,
                        taskCreationOptions: TaskCreationOptions.LongRunning)), // long running so another thread is used to run handlers ASAP
                (typeof(Event1Data), new List<Lazy<IHandler>> { new Lazy<IHandler>(new DelayedEvent1Handler(delay)) }));
            var event1 = new DomainEvent<Event1Data>("event-1", new Event1Data());

            var sw = Stopwatch.StartNew();
            await notifier.Notify(event1);
            sw.Stop();

            Assert.IsTrue(sw.Elapsed < delay);
            Console.WriteLine($"Outer task completed in: {sw.Elapsed}");

            //wait for inner tasks to finish
            await Task.Delay(delay);
        }

        [TestMethod]
        public async Task Notify_WithAsyncBehavior_HavingTaskSink_ShouldCatchRawHandlerExceptions()
        {
            Exception sinkException = null;
            Action<Task> sink = task =>
            {
                sinkException = task.Exception.InnerExceptions[0];
            };

            var fault = new InvalidProgramException("something");
            var notifier = new DomainEventNotifier(
                new Options(
                    new AsyncOptions(
                        taskSink: sink,
                        cancellationToken: null,
                        taskCreationOptions: TaskCreationOptions.LongRunning)), // long running so another thread is used to run handlers ASAP
                (typeof(Event1Data), new List<Lazy<IHandler>> { new Lazy<IHandler>(new FaultingEvent1Handler(fault)) }));
            var event1 = new DomainEvent<Event1Data>("event-1", new Event1Data());

            await notifier.Notify(event1);

            // wait for the notifiers to complete
            while(sinkException == null)
                await Task.Delay(TimeSpan.FromMilliseconds(10));

            Assert.AreEqual(fault, sinkException);
        }

        [TestMethod]
        public async Task Notify_WithAsyncBehavior_HavingErrorConsumers_ShouldConsumeExceptions()
        {
            HandlerException[] handlerErrors = null;
            Action<HandlerException[]> errorConsumer = errors =>
            {
                handlerErrors = errors;
            };
            var fault = new InvalidProgramException("something");
            var notifier = new DomainEventNotifier(
                new Options(
                    new AsyncOptions(
                        cancellationToken: null,
                        taskCreationOptions: TaskCreationOptions.LongRunning,
                        errorConsumer)), // long running so another thread is used to run handlers ASAP
                (typeof(Event1Data), new List<Lazy<IHandler>>
                {
                    new Lazy<IHandler>(new FaultedOperationEvent1Handler(fault)),
                    new Lazy<IHandler>(new FaultedOperationEvent1Handler(fault))
                }));
            var event1 = new DomainEvent<Event1Data>("event-1", new Event1Data());

            await notifier.Notify(event1);

            // wait for the notifiers to complete
            while (handlerErrors == null)
                await Task.Delay(TimeSpan.FromMilliseconds(5));

            Assert.AreEqual(2, handlerErrors.Length);
        }

        [TestMethod]
        public async Task Notify_WithAsyncBehavior_HavingFaultyErrorConsumers_ShouldConsumeExceptions()
        {
            AggregateException sinkException = null;
            Action<Task> sink = task =>
            {
                sinkException = task.Exception;
            };
            Action<HandlerException[]> errorConsumer = errors => { };
            Action<HandlerException[]> errorConsumer2 = errors =>
            {
                throw new InvalidCastException();
            };
            var fault = new InvalidProgramException("something");
            var notifier = new DomainEventNotifier(
                new Options(
                    new AsyncOptions(
                        taskSink: sink,
                        cancellationToken: null,
                        taskCreationOptions: TaskCreationOptions.LongRunning,
                        errorConsumer,
                        errorConsumer2,
                        errorConsumer2,
                        errorConsumer2)), // long running so another thread is used to run handlers ASAP
                (typeof(Event1Data), new List<Lazy<IHandler>>
                {
                    new Lazy<IHandler>(new FaultedOperationEvent1Handler(fault)),
                    new Lazy<IHandler>(new FaultedOperationEvent1Handler(fault))
                }));
            var event1 = new DomainEvent<Event1Data>("event-1", new Event1Data());

            await notifier.Notify(event1);

            // wait for the notifiers to complete
            while (sinkException == null)
                await Task.Delay(TimeSpan.FromMilliseconds(5));

            Assert.AreEqual(4, sinkException.InnerException.As<AggregateException>().InnerExceptions.Count);
        }
    }
}
