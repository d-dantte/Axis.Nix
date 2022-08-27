using Axis.Luna.Common;
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
    /// 
    /// </summary>
    public class DomainEventNotifier: IDomainEventNotifier
    {
        private readonly Dictionary<Type, IHandler[]> handlerMaps = new Dictionary<Type, IHandler[]>();
        private readonly Options configOptions; 
        private readonly TaskFactory notifyTaskFactory;

        #region Constructors
        public DomainEventNotifier(Options options, IEnumerable<(Type eventType, List<IHandler> handlers)> handlers = null)
        {
            configOptions = options; //.ThrowIfDefault(new ArgumentException($"Default {nameof(options)} instance is invalid"));

            var scheduler = configOptions.AsyncBehavior?.SchedulerProvider();

            notifyTaskFactory = configOptions.IsAsyncBehaviorEnabled
                ? new TaskFactory(
                    configOptions.AsyncBehavior?.CancellationToken ?? default,
                    configOptions.AsyncBehavior?.TaskCreationOptions ?? TaskCreationOptions.None,
                    TaskContinuationOptions.None,
                    scheduler)
                : null;

            handlers?.ForAll(pair =>
            {
                handlerMaps[pair.eventType] =
                    pair.handlers?
                        .Where(handler => handler != null) //<-- filter out nulls
                        .ToArray()
                    ?? throw new ArgumentNullException("Null handler list");
            });
        }

        public DomainEventNotifier(Options options, params (Type, List<IHandler>)[] handlers)
            : this(options, handlers?.AsEnumerable())
        { }
        #endregion

        public Operation Notify<TEvent>(TEvent @event)
        where TEvent : IDomainEvent
        {
            if (!handlerMaps.TryGetValue(typeof(TEvent), out var handlers))
                return Operation.Fail(new HandlerNotFoundException(typeof(TEvent)));

            if (@event == null)
                return Operation.Fail(new ArgumentNullException(nameof(@event)));

            var typedHandlers = handlers.Cast<IEventHandler<TEvent>>().ToArray();

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
                                    .Select(handler => handler
                                        .HandleEvent(@event) //<-- note that handlers MUST NOT throw exceptions from the "HandleEvent" method
                                        .MapError(error => new HandlerException(handler, @event, error.GetException()).Throw()))
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
                            if (configOptions.AsyncBehavior?.TaskSink == null)
                                await task;

                            else configOptions.AsyncBehavior?.TaskSink.Invoke(task);
                        })
                        .Unwrap();

                    return Operation.FromVoid();
                }
                else
                {
                    return typedHandlers
                        .Select(handler => handler.HandleEvent(@event)) //<-- note that handlers MUST NOT throw exceptions from the "HandleEvent" method
                        .Fold();
                }
            }

            return Operation.FromVoid();
        }
    }
}
