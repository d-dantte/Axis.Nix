using App.Metrics;
using App.Metrics.Apdex;
using App.Metrics.Counter;
using App.Metrics.Gauge;
using App.Metrics.Histogram;
using App.Metrics.Meter;
using App.Metrics.Timer;
using Axis.Nix.Builder;
using Axis.Nix.Event;
using Axis.Nix.Metrics.AppMetrics.Handlers;
using Axis.Nix.Metrics.AppMetrics.Events;
using Axis.Nix.Metrics.AppMetrics.Interceptors;
using Castle.DynamicProxy;
using System;

namespace Axis.Nix.Metrics
{
    public static class AppMetricsEventHandlerRegistry
    {
        /// <summary>
        /// Registers a new Event handler that collects <see cref="ICounterMetric"/> data for the given event
        /// </summary>
        /// <typeparam name="TEventData">The event data type</typeparam>
        /// <param name="metrics">The metrics instance</param>
        /// <param name="notifierBuilder">The notifier builder instance</param>
        /// <param name="metricsCollector">The delegate that collects the metrics from the given event</param>
        /// <param name="metricsPredicate">The optional predicate that filters events</param>
        public static void RegisterCounterCollector<TEventData>(
            IMetrics metrics,
            NotifierBuilder notifierBuilder,
            Action<DomainEvent<TEventData>, IMeasureCounterMetrics> metricsCollector,
            Func<DomainEvent<TEventData>, bool> metricsPredicate = null)
        {
            _ = notifierBuilder.WithEventHandler<TEventData, CounterMetricsHandler<TEventData>>(
                new CounterMetricsHandler<TEventData>(
                    metrics,
                    metricsCollector,
                    metricsPredicate));
        }

        /// <summary>
        /// Registers a new Event handler that collects <see cref="IGaugeMetrics"/> data for the given event
        /// </summary>
        /// <typeparam name="TEventData">The event data type</typeparam>
        /// <param name="metrics">The metrics instance</param>
        /// <param name="notifierBuilder">The notifier builder instance</param>
        /// <param name="metricsCollector">The delegate that collects the metrics from the given event</param>
        /// <param name="metricsPredicate">The optional predicate that filters events</param>
        public static void RegisterGaugeCollector<TEventData>(
            IMetrics metrics,
            NotifierBuilder notifierBuilder,
            Action<DomainEvent<TEventData>, IMeasureGaugeMetrics> metricsCollector,
            Func<DomainEvent<TEventData>, bool> metricsPredicate = null)
        {
            _ = notifierBuilder.WithEventHandler<TEventData, GaugeMetricsHandler<TEventData>>(
                new GaugeMetricsHandler<TEventData>(
                    metrics,
                    metricsCollector,
                    metricsPredicate));
        }

        /// <summary>
        /// Registers a new Event handler that collects <see cref="IHistogramMetric"/> data for the given event
        /// </summary>
        /// <typeparam name="TEventData">The event data type</typeparam>
        /// <param name="metrics">The metrics instance</param>
        /// <param name="notifierBuilder">The notifier builder instance</param>
        /// <param name="metricsCollector">The delegate that collects the metrics from the given event</param>
        /// <param name="metricsPredicate">The optional predicate that filters events</param>
        public static void RegisterHistogramCollector<TEventData>(
            IMetrics metrics,
            NotifierBuilder notifierBuilder,
            Action<DomainEvent<TEventData>, IMeasureHistogramMetrics> metricsCollector,
            Func<DomainEvent<TEventData>, bool> metricsPredicate = null)
        {
            _ = notifierBuilder.WithEventHandler<TEventData, HistogramMetricsHandler<TEventData>>(
                new HistogramMetricsHandler<TEventData>(
                    metrics,
                    metricsCollector,
                    metricsPredicate));
        }

        /// <summary>
        /// Registers a new Event handler that collects <see cref="IMeterMetric"/> data for the given event
        /// </summary>
        /// <typeparam name="TEventData">The event data type</typeparam>
        /// <param name="metrics">The metrics instance</param>
        /// <param name="notifierBuilder">The notifier builder instance</param>
        /// <param name="metricsCollector">The delegate that collects the metrics from the given event</param>
        /// <param name="metricsPredicate">The optional predicate that filters events</param>
        public static void RegisterMeterCollector<TEventData>(
            IMetrics metrics,
            NotifierBuilder notifierBuilder,
            Action<DomainEvent<TEventData>, IMeasureMeterMetrics> metricsCollector,
            Func<DomainEvent<TEventData>, bool> metricsPredicate = null)
        {
            _ = notifierBuilder.WithEventHandler<TEventData, MeterMetricsHandler<TEventData>>(
                new MeterMetricsHandler<TEventData>(
                    metrics,
                    metricsCollector,
                    metricsPredicate));
        }

        /// <summary>
        /// Registers a new Event handler that collects <see cref="ITimerMetric"/> data for the given event
        /// </summary>
        /// <typeparam name="TEventData">The event data type</typeparam>
        /// <param name="metrics">The metrics instance</param>
        /// <param name="notifierBuilder">The notifier builder instance</param>
        /// <param name="metricsCollector">The delegate that collects the metrics from the given event</param>
        /// <param name="metricsPredicate">The optional predicate that filters events</param>
        public static void RegisterTimerCollector<TEventData>(
            IMetrics metrics,
            NotifierBuilder notifierBuilder,
            Action<DomainEvent<TEventData>, IMeasureTimerMetrics> metricsCollector,
            Func<DomainEvent<TEventData>, bool> metricsPredicate = null)
        {
            _ = notifierBuilder.WithEventHandler<TEventData, TimerMetricsHandler<TEventData>>(
                new TimerMetricsHandler<TEventData>(
                    metrics,
                    metricsCollector,
                    metricsPredicate));
        }

        /// <summary>
        /// Registers a new Event handler that collects <see cref="IApdexMetric"/> data for the given event
        /// </summary>
        /// <typeparam name="TEventData">The event data type</typeparam>
        /// <param name="metrics">The metrics instance</param>
        /// <param name="notifierBuilder">The notifier builder instance</param>
        /// <param name="metricsCollector">The delegate that collects the metrics from the given event</param>
        /// <param name="metricsPredicate">The optional predicate that filters events</param>
        public static void RegisterTimerCollector<TEventData>(
            IMetrics metrics,
            NotifierBuilder notifierBuilder,
            Action<DomainEvent<TEventData>, IMeasureApdexMetrics> metricsCollector,
            Func<DomainEvent<TEventData>, bool> metricsPredicate = null)
        {
            _ = notifierBuilder.WithEventHandler<TEventData, ApdexMetricsHandler<TEventData>>(
                new ApdexMetricsHandler<TEventData>(
                    metrics,
                    metricsCollector,
                    metricsPredicate));
        }

        /// <summary>
        /// Registers a new metrics handler that collects any type of metric data for the given event.
        /// <para>
        /// Also creates an <see cref="IInterceptor"/> instance that raises the events. This interceptor can then be used on the method(s) 
        /// where <see cref="InvocationEvent"/> needs to be raised.
        /// </para>
        /// <para>
        /// Note: the <paramref name="invocationPredicate"/> is interpreted as '<c>true</c>' if it is null.
        /// </para>
        /// </summary>
        /// <param name="metrics">The metrics instance</param>
        /// <param name="notifierBuilder">The notifier builder instance</param>
        /// <param name="eventNameProducer">Generates the name given to the events being raised, based on the <see cref="IInvocation"/></param>
        /// <param name="metricsCollector">The delegate that collects the metrics from the given event</param>
        /// <param name="metricsPredicate">The optional predicate that filters events</param>
        /// <param name="invocationPredicate">The optoinal predicate that filters <see cref="IInvocation"/> instances</param>
        /// <returns></returns>
        public static InvocationEventBroadcaster RegisterInvocationEventMetricsCollector(
            IMetrics metrics,
            NotifierBuilder notifierBuilder,
            Func<IInvocation, string> eventNameProducer,
            Action<DomainEvent<IInvocationEvent>, IMeasureMetrics> metricsCollector,
            Func<DomainEvent<IInvocationEvent>, bool> metricsPredicate = null,
            Func<IInvocation, bool> invocationPredicate = null)
        {
            _ = notifierBuilder.WithEventHandler<IInvocationEvent, MetricsHandler<IInvocationEvent>>(
                new MetricsHandler<IInvocationEvent>(
                    metrics,
                    metricsCollector,
                    metricsPredicate));

            return new InvocationEventBroadcaster(
                eventNameProducer,
                notifierBuilder.Instance,
                invocationPredicate);
        }
    }
}
