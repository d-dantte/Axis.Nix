﻿using App.Metrics;
using Axis.Luna.Operation;
using Axis.Nix.Event;
using Axis.Nix.Handler;
using System;

namespace Axis.Nix.Metrics.AppMetrics.Handlers
{
    internal class MetricsHandler<TEventData>: IEventHandler<TEventData>
    {
        private Func<DomainEvent<TEventData>, bool> _metricsPredicate;

        private Action<DomainEvent<TEventData>, IMeasureMetrics> _metricsCollector;

        private IMetrics _metrics;


        public MetricsHandler(
            IMetrics metrics,
            Action<DomainEvent<TEventData>, IMeasureMetrics> metricsCollector,
            Func<DomainEvent<TEventData>, bool> metricsPredicate = null)
        {
            _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
            _metricsCollector = metricsCollector ?? throw new ArgumentNullException(nameof(metricsCollector));
            _metricsPredicate = metricsPredicate;
        }

        public bool CanHandle(DomainEvent<TEventData> @event) => _metricsPredicate?.Invoke(@event) ?? true;

        public IOperation HandleEvent(DomainEvent<TEventData> @event)
            => Operation.Try(() => _metricsCollector.Invoke(@event, _metrics.Measure));
    }
}