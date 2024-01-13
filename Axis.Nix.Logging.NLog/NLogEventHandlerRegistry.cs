using Axis.Nix.Builder;
using Axis.Nix.Event;
using Axis.Nix.Logging.NLog.Handlers;
using NLog;
using System;

namespace Axis.Nix.Logging.NLog
{
    public static class NLogEventHandlerRegistry
    {
        public static void RegisterEventLogger<TEventData>(
            ILogger logger,
            NotifierBuilder notifierBuilder,
            Action<DomainEvent<TEventData>, ILogger> eventLogger,
            Func<DomainEvent<TEventData>, bool> loggerPredicate = null)
        {
            _ = notifierBuilder.WithEventHandler<TEventData, NLogEventHandler<TEventData>>(
                new NLogEventHandler<TEventData>(
                    logger,
                    eventLogger,
                    loggerPredicate));
        }
    }

}
