using Axis.Nix.Event;
using Axis.Nix.Metrics.AppMetrics.Events;
using Castle.DynamicProxy;
using System;

namespace Axis.Nix.Metrics.AppMetrics.Interceptors
{
    public class InvocationEventBroadcaster : IInterceptor
    {
        private readonly Func<IInvocation, string> _eventNameProducer;
        private readonly Func<DomainEventNotifier> _notifierProvider;
        private readonly Func<IInvocation, bool> _invocationPredicate;

        /// <summary>
        /// Creaates a new instance of this type. Note that if the <paramref name="invocationPredicate"/> is null, it is interpreted as a '<c>true</c>' evaluation.
        /// </summary>
        /// <param name="eventNameProducer"></param>
        /// <param name="notifierProvider"></param>
        /// <param name="invocationPredicate">The optional invocation predicate.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public InvocationEventBroadcaster(
            Func<IInvocation, string> eventNameProducer,
            Func<DomainEventNotifier> notifierProvider,
            Func<IInvocation, bool> invocationPredicate = null)
        {
            _eventNameProducer = eventNameProducer ?? throw new ArgumentNullException(nameof(eventNameProducer));
            _notifierProvider = notifierProvider ?? throw new ArgumentNullException(nameof(notifierProvider));
            _invocationPredicate = invocationPredicate;
        }

        public void Intercept(IInvocation invocation)
        {
            if (_invocationPredicate?.Invoke(invocation) == false)
                invocation.Proceed();

            else
            {
                Exception exception = null;
                System.Diagnostics.Stopwatch stopwatch = null;
                try
                {
                    stopwatch = System.Diagnostics.Stopwatch.StartNew();
                    invocation.Proceed();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                finally
                {
                    stopwatch.Stop();
                    var eventData = exception is null
                        ? IInvocationEvent.Of(
                            invocation.InvocationTarget,
                            invocation.GetConcreteMethodInvocationTarget(),
                            invocation.Arguments,
                            invocation.ReturnValue,
                            stopwatch.Elapsed)
                        : IInvocationEvent.Of(
                            invocation.InvocationTarget,
                            invocation.GetConcreteMethodInvocationTarget(),
                            invocation.Arguments,
                            exception,
                            stopwatch.Elapsed);

                    _ = _notifierProvider
                        .Invoke()
                        .Notify(
                            new DomainEvent<IInvocationEvent>(
                                _eventNameProducer.Invoke(invocation),
                                eventData));
                }
            }
        }
    }
}
