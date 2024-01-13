using Axis.Luna.Operation;
using Axis.Nix.Event;
using Axis.Nix.Handler;
using Serilog;
using System;

namespace Axis.Nix.Logging.Serilog.Handlers
{
    public class SerilogEventHandler<TEventData> : IEventHandler<TEventData>
    {
        private Func<DomainEvent<TEventData>, bool> _loggerPredicate;

        private Action<DomainEvent<TEventData>, ILogger> _eventLogger;

        private ILogger _logger;


        public SerilogEventHandler(
            ILogger logger,
            Action<DomainEvent<TEventData>, ILogger> eventLogger,
            Func<DomainEvent<TEventData>, bool> loggerPredicate = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventLogger = eventLogger ?? throw new ArgumentNullException(nameof(eventLogger));
            _loggerPredicate = loggerPredicate;
        }

        public bool CanHandle(DomainEvent<TEventData> @event) => _loggerPredicate?.Invoke(@event) ?? true;

        public IOperation HandleEvent(DomainEvent<TEventData> @event)
            => Operation.Try(() => _eventLogger.Invoke(@event, _logger));
    }
}
