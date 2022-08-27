using Axis.Luna.Operation;
using Axis.Nix.Event;
using Axis.Nix.Handler;
using System;

namespace Axis.Nix.Tests.Types
{
    public class Event2: IDomainEvent
    {
        public string ID { get; set; }

        public string Name => typeof(Event2).FullName;

        public override string ToString() => $"{{ID:{ID}, Name:{Name}}}";
    }

    public class EventHandler2 : IEventHandler<Event2>
    {
        public Operation HandleEvent(Event2 @event) => Operation.Try(() =>
        {
            Console.WriteLine($"{@event} was handled by {typeof(EventHandler2)}");
        });
    }
}
