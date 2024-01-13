using App.Metrics;
using App.Metrics.Gauge;
using Axis.Luna.Operation;
using Axis.Nix.Event;
using Axis.Nix.Handler;
using System;

namespace Axis.Nix.Metrics.AppMetrics.Handlers
{
    internal class GaugeMetricsHandler<TEventData>: IEventHandler<TEventData>
    {
        private Func<DomainEvent<TEventData>, bool> _metricsPredicate;

        private Action<DomainEvent<TEventData>, IMeasureGaugeMetrics> _metricsCollector;

        private IMetrics _metrics;


        public GaugeMetricsHandler(
            IMetrics metrics,
            Action<DomainEvent<TEventData>, IMeasureGaugeMetrics> metricsCollector,
            Func<DomainEvent<TEventData>, bool> metricsPredicate = null)
        {
            _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
            _metricsCollector = metricsCollector ?? throw new ArgumentNullException(nameof(metricsCollector));
            _metricsPredicate = metricsPredicate;
        }

        public bool CanHandle(DomainEvent<TEventData> @event) => _metricsPredicate?.Invoke(@event) ?? true;

        public IOperation HandleEvent(DomainEvent<TEventData> @event)
            => Operation.Try(() => _metricsCollector.Invoke(@event, _metrics.Measure.Gauge));
    }
}
