using Axis.Nix.Builder;
using Axis.Nix.Event;
using Axis.Nix.Logging.Serilog.Handlers;
using Serilog;
using System;

namespace Axis.Nix.Logging.Serilog
{
    public static class SerilogEventHandlerRegistry
    {
        public static void RegisterEventLogger<TEventData>(
            ILogger logger,
            NotifierBuilder notifierBuilder,
            Action<DomainEvent<TEventData>, ILogger> eventLogger,
            Func<DomainEvent<TEventData>, bool> loggerPredicate = null)
        {
            _ = notifierBuilder.WithEventHandler<TEventData, SerilogEventHandler<TEventData>>(
                new SerilogEventHandler<TEventData>(
                    logger,
                    eventLogger,
                    loggerPredicate));
        }
    }
}
