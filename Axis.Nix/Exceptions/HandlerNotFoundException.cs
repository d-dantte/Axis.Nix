using Axis.Luna.Extensions;
using Axis.Nix.Event;
using System;

namespace Axis.Nix.Exceptions
{
    public class HandlerNotFoundException: Exception
    {
        public Type EventDataType { get; }

        public HandlerNotFoundException(Type eventType)
            :base($"Handler not found for event type: {eventType}")
        {
            EventDataType = eventType;
        }
    }
}
