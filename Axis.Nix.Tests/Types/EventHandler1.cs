using Axis.Luna.Operation;
using Axis.Nix.Event;
using Axis.Nix.Handler;
using System;

namespace Axis.Nix.Tests.Types
{
    public class Event1: IDomainEvent
    {
        public string ID { get; set; }

        public string Name => typeof(Event1).FullName;

        public override string ToString() => $"{{ID:{ID}, Name:{Name}}}";
    }

    public class EventHandler1 : IEventHandler<Event1>
    {
        public Operation HandleEvent(Event1 @event) => Operation.Try(() =>
        {
            Console.WriteLine($"{@event.ToString()} was handled by {typeof(EventHandler1)}");
        });
    }
}
