using Axis.Luna.Extensions;
using Axis.Nix.Event;
using System;

namespace Axis.Nix.Exceptions
{
    public class HandlerNotFoundException: Exception
    {
        public Type EventType { get; }

        public HandlerNotFoundException(Type eventType)
            :base($"Handler not found for event type: {eventType}")
        {
            EventType = eventType.ThrowIf(
                IsNotEventType,
                new Exception($"Specified type must be  {typeof(IDomainEvent)}"));
        }

        private static bool IsNotEventType(Type type)
        {
            return !type.Implements(typeof(IDomainEvent));
        }
    }
}
