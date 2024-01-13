using Axis.Luna.Operation;
using Axis.Nix.Event;
using Axis.Nix.Handler;
using System;

namespace Axis.Nix.Tests.Types
{
    public struct Event2Data
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class Event2Handler : IEventHandler<Event2Data>
    {
        public bool CanHandle(DomainEvent<Event2Data> @event) => @event != default;

        public IOperation HandleEvent(DomainEvent<Event2Data> @event)
        => Operation.Try(() =>
        {
            Console.WriteLine($"{@event.GetType()} was handled in {typeof(Event2Handler)}");
        });
    }
}
