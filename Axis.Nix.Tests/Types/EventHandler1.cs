using Axis.Luna.Operation;
using Axis.Nix.Event;
using Axis.Nix.Handler;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Axis.Nix.Tests.Types
{
    public class Event1Data
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class Event1Handler : IEventHandler<Event1Data>
    {
        private int count = 0;

        public int HandleCount => count;

        public bool CanHandle(DomainEvent<Event1Data> @event) => @event != default;

        public IOperation HandleEvent(DomainEvent<Event1Data> @event)
        => Operation.Try(() =>
        {
            Interlocked.Increment(ref count);
            Console.WriteLine($"{@event.GetType()} was handled in {typeof(Event1Handler)}");
        });
    }

    public class Event1PrefixHandler : IEventHandler<Event1Data>
    {
        public string Prefix { get; }

        public Event1PrefixHandler(string prefix)
        {
            Prefix = prefix;
        }

        public bool CanHandle(DomainEvent<Event1Data> @event) => @event != default && @event.Name.StartsWith(Prefix);

        public IOperation HandleEvent(DomainEvent<Event1Data> @event)
        => Operation.Try(() =>
        {
            Console.WriteLine($"{@event.GetType()} was handled in {typeof(Event1PrefixHandler)}");
        });
    }

    public class Event1Handler2 : IEventHandler<Event1Data>
    {
        public bool CanHandle(DomainEvent<Event1Data> @event) => @event != default;

        public IOperation HandleEvent(DomainEvent<Event1Data> @event)
        => Operation.Try(() =>
        {
            Console.WriteLine($"{@event.GetType()} was handled in {typeof(Event1Handler2)}");
        });
    }

    public class FaultingEvent1Handler : IEventHandler<Event1Data>
    {
        public Exception Fault { get; }

        public FaultingEvent1Handler(Exception exception)
        {
            Fault = exception;
        }

        public bool CanHandle(DomainEvent<Event1Data> @event) => @event != default;

        public IOperation HandleEvent(DomainEvent<Event1Data> @event) => throw Fault ?? new Exception("faulted");
    }

    public class DelayedEvent1Handler : IEventHandler<Event1Data>
    {
        public TimeSpan Delay { get; }

        public DelayedEvent1Handler(TimeSpan delay)
        {
            Delay = delay;
        }

        public bool CanHandle(DomainEvent<Event1Data> @event) => @event != default;

        public IOperation HandleEvent(DomainEvent<Event1Data> @event)
        => Operation.Try(async () =>
        {
            // give the outer task the opportunity to run first. Note though that if it awaited, this wouldn't matter
            await Task.Yield();

            Console.WriteLine($"Delaying for: {Delay}");
            await Task.Delay(Delay);
            Console.WriteLine($"{@event.GetType()} was handled in {typeof(DelayedEvent1Handler)}");
        });
    }

    public class FaultedOperationEvent1Handler: IEventHandler<Event1Data>
    {
        public Exception Fault { get; }

        public FaultedOperationEvent1Handler(Exception exception)
        {
            Fault = exception;
        }

        public bool CanHandle(DomainEvent<Event1Data> @event) => @event != default;

        public IOperation HandleEvent(DomainEvent<Event1Data> @event)
        => Fault == null
            ? Operation.Fail(new Exception("faulted"))
            : Operation.Fail(Fault);
    }
}
