using Axis.Nix.Event;
using Axis.Nix.Handler;
using System;

namespace Axis.Nix.Exceptions
{
    public class HandlerException: Exception
    {
        internal IHandler Handler { get; }

        internal IDomainEvent Event { get; }

        public HandlerException(IHandler handler, IDomainEvent @event, Exception inner)
        :base("Handler threw an exception", inner)
        {
            Handler = handler;
            Event = @event;
        }
    }
}
