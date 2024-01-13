using Axis.Luna.Extensions;
using Axis.Luna.Operation;
using Axis.Nix.Configuration;
using Axis.Nix.Event;
using Axis.Nix.Exceptions;
using Axis.Nix.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axis.Nix
{
    /// <summary>
    /// Event notifier that encapsulates the registered events and handlers, and routes the raised event to it's registered handlers.
    /// </summary>
    public class DomainEventNotifier
    {
        private readonly Dictionary<Type, Lazy<IHandler>[]> handlerMaps = new Dictionary<Type, Lazy<IHandler>[]>();
        private readonly Options configOptions; 
        private readonly TaskFactory notifyTaskFactory;

        #region Constructors
        public DomainEventNotifier(
            Options options,
            IEnumerable<(Type eventType, List<Lazy<IHandler>> handlers)> handlers)
        {
            configOptions = options;

            var scheduler = configOptions.AsyncBehavior?.SchedulerProvider?.Invoke() ?? AsyncOptions.DefaultSchedulerProvider();

            notifyTaskFactory = configOptions.IsAsyncBehaviorEnabled
                ? new TaskFactory(
                    configOptions.AsyncBehavior?.CancellationToken ?? default,
                    configOptions.AsyncBehavior?.TaskCreationOptions ?? TaskCreationOptions.None,
                    TaskContinuationOptions.None,
                    scheduler)
                : null;

            handlers
                .ThrowIfNull(new ArgumentNullException(nameof(handlers)))
                .ForAll(pair =>
                {
                    handlerMaps[pair.eventType] =
                        pair.handlers?
                            .Select(handler => handler.ThrowIfNull(new ArgumentException("handler list cannot contain null"))) //<-- filter out nulls
                            .ToArray()
                        ?? throw new ArgumentNullException("Null handler list");
                });
        }

        public DomainEventNotifier(Options options, params (Type, List<Lazy<IHandler>>)[] handlers)
            : this(options, handlers?.AsEnumerable())
        { }

        public DomainEventNotifier(Options options, IEnumerable<KeyValuePair<Type, List<Lazy<IHandler>>>> handlers)
            : this(options, handlers?.Select(kvp => (kvp.Key, kvp.Value)).AsEnumerable())
        { }
        #endregion

        /// <summary>
        /// Notify handlers that an event has been raised.
        /// <para>
        /// NOTE: all handlers get a chance to run - i.e, exceptions do not interrupt the process.
        /// </para>
        /// <para>
        /// ALSO NOTE: It is PERFECTLY SAFE to discard the <see cref="IOperation"/> instance returned from this method without awaiting or resolving it.
        /// </para>
        /// </summary>
        /// <typeparam name="TEventData"></typeparam>
        /// <param name="event"></param>
        /// <returns></returns>
        /// <exception cref="AggregateException"></exception>
        public IOperation Notify<TEventData>(DomainEvent<TEventData> @event)
        {
            if (!handlerMaps.TryGetValue(typeof(TEventData), out var handlers))
                return Operation.Fail(new HandlerNotFoundException(typeof(TEventData)));

            if (@event == default)
                return Operation.Fail(new ArgumentException(nameof(@event)));

            var typedHandlers = handlers
                .Select(_h => _h.Value)
                .Cast<IEventHandler<TEventData>>()
                .Where(_h => _h.CanHandle(@event))
                .ToArray();

            if (typedHandlers.Length > 0)
            {
                // if task scheduling is enabled for notifications, schedule a task to execute the handlers
                if (configOptions.IsAsyncBehaviorEnabled)
                {
                    // This is essentially a fire and forget situation. But we allow for situations where we register callbacks for processing issues from the threads to 
                    // possibly log the encountered errors, etc.
                    _ = notifyTaskFactory
                        .StartNew(async () =>
                        {
                            try
                            {
                                await typedHandlers
                                    .Select(handler =>
                                    {
                                        return handler
                                            .HandleEvent(@event) //<-- raw exceptions thrown from the handlers are folded into the final operation
                                            .MapError(error => new HandlerException(handler, @event, error.GetException()).Throw());
                                    })
                                    .Fold();
                            }
                            catch (AggregateException e)
                            {
                                // if no error consumers exist, throw the exception
                                if (configOptions.AsyncBehavior?.ErrorConsumers.Length == 0)
                                    throw;

                                // cast the exceptions
                                var handlerExceptions = e.InnerExceptions
                                    .Cast<HandlerException>()
                                    .ToArray();

                                // notify error consumers. Note that every consumer MUST get a chance to consume the error.
                                var errorList = new List<Exception>();
                                configOptions.AsyncBehavior?.ErrorConsumers.ForAll(consumer =>
                                {
                                    try
                                    {
                                        consumer.Invoke(handlerExceptions);
                                    }
                                    catch (Exception inner)
                                    {
                                        errorList.Add(inner);
                                    }
                                });

                                if (errorList.Count > 0)
                                {
                                    errorList.Add(e);
                                    throw new AggregateException(errorList);
                                }
                            }
                        })
                        .Unwrap()
                        .ContinueWith(async task =>
                        {
                            // note that exceptions thrown from handlers that are not encapsulated in their IOperations
                            // come directly here.

                            if (configOptions.AsyncBehavior?.TaskSink == null)
                                await task;

                            else configOptions.AsyncBehavior?.TaskSink.Invoke(task);
                        })
                        .Unwrap();

                    return Operation.FromVoid();
                }
                else
                {
                    return Operation.Try(async () => 
                        await typedHandlers
                            .Select(handler => handler.HandleEvent(@event)) //<-- raw exceptions thrown from the handlers are folded into the final operation
                            .Fold());
                }
            }

            // no handlers registered
            return Operation.FromVoid();
        }

        /// <summary>
        /// Gets an array of all registered event data types
        /// </summary>
        public Type[] RegisteredEvents() => handlerMaps.Keys.ToArray();
    }
}
