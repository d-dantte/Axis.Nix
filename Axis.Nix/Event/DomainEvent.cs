using Axis.Luna.Extensions;
using System;

namespace Axis.Nix.Event
{
    /// <summary>
    /// Represents a domain-level event, raised by the domain logic for handlers to respond to. Responding to events happens either in the background
    /// (if <see cref="Configuration.Options.AsyncBehavior"/> is set), or in the current thread.
    /// </summary>
    /// <typeparam name="TEventData"></typeparam>
    public readonly struct DomainEvent<TEventData>: IDomainEvent
    {
        /// <summary>
        /// The name for the event. No constraint is placed on this value except that it cannot be null or empty.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The event data, encapsulating any information that the event is comprised of.
        /// </summary>
        public TEventData Data { get; }


        public DomainEvent(string eventName, TEventData data)
        {
            Name = eventName.ThrowIf(
                string.IsNullOrWhiteSpace,
                new ArgumentException($"Invalid {nameof(eventName)}"));

            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public override int GetHashCode() => HashCode.Combine(Name, Data);

        public override bool Equals(object obj)
        {
            return obj is DomainEvent<TEventData> other
                && other.Name.NullOrEquals(Name)
                && other.Data.NullOrEquals(Data);
        }

        public static bool operator ==(DomainEvent<TEventData> first, DomainEvent<TEventData> second) => first.Equals(second);
        public static bool operator !=(DomainEvent<TEventData> first, DomainEvent<TEventData> second) => !first.Equals(second);
    }
}
