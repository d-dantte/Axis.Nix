using Axis.Luna.Extensions;
using Axis.Luna.Operation;
using Axis.Nix.Configuration;
using Axis.Nix.Event;
using Axis.Nix.Handler;
using Axis.Proteus.IoC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Nix.Builder
{
    /// <summary>
    /// Builder for the <see cref="DomainEventNotifier"/>
    /// </summary>
    public sealed class NotifierBuilder
    {
        private readonly Dictionary<Type, HashSet<Type>> _handlerTypeMap = new Dictionary<Type, HashSet<Type>>();
        private readonly Dictionary<Type, List<IHandler>> _handlerInstanceMap = new Dictionary<Type, List<IHandler>>();
        private Options _options;
        private DomainEventNotifier _notifier;

        private NotifierBuilder()
        { }

        public static NotifierBuilder NewBuilder() => new NotifierBuilder();

        /// <summary>
        /// The current Options for the <see cref="DomainEventNotifier"/>
        /// </summary>
        /// <returns></returns>
        public Options BuilderOptions() => _options;

        /// <summary>
        /// Set options for the <see cref="DomainEventNotifier"/>
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public NotifierBuilder Configure(Options options)
        {
            ValidateBuilder();

            _options = options;

            return this;
        }

        /// <summary>
        /// Adds an event handler type to the builder, bound to the given event data type.
        /// <para>
        /// NOTE: event handlers are singleton instances, so they should be written with thread safety in mind.
        /// </para>
        /// </summary>
        /// <typeparam name="TEventData">The event data type</typeparam>
        /// <typeparam name="THandler">The handler type</typeparam>
        /// <param name="registrar">The IoC registration container</param>
        /// <exception cref="ArgumentNullException">If <paramref name="registrar"/> is null</exception>
        /// <exception cref="ArgumentException">If duplicate <typeparamref name="TEventData"/>s are detected</exception>
        public NotifierBuilder WithEventHandler<TEventData, THandler>(IRegistrarContract registrar)
        where THandler: class, IEventHandler<TEventData>
        {
            ValidateBuilder();

            if (registrar == null)
                throw new ArgumentNullException(nameof(registrar));

            if (!_handlerTypeMap
                .GetOrAdd(typeof(TEventData), eventDataType => new HashSet<Type>())
                .Add(typeof(THandler)))
                throw new ArgumentException($"Duplicate event handler detected");

            _ = registrar.AddRegistration<THandler>(RegistryScope.Singleton);

            return this;
        }

        /// <summary>
        /// Adds an event handler instance to the builder, bound to the given event data type.
        /// </summary>
        /// <typeparam name="TEventData">The event data type</typeparam>
        /// <typeparam name="THandler">The handler type</typeparam>
        /// <param name="handlerInstance">The handler instance</param>
        /// <exception cref="ArgumentNullException">if <paramref name="handlerInstance"/> is null</exception>
        public NotifierBuilder WithEventHandler<TEventData, THandler>(THandler handlerInstance)
        where THandler: class, IEventHandler<TEventData>
        {
            ValidateBuilder();

            if (handlerInstance == null)
                throw new ArgumentNullException(nameof(handlerInstance));

            _handlerInstanceMap
                .GetOrAdd(typeof(TEventData), eventDataType => new List<IHandler>())
                .Add(handlerInstance);

            return this;
        }

        /// <summary>
        /// Adds a new event handler function to the builder, bound to the given event data type.
        /// </summary>
        /// <typeparam name="TEventData">The event data type</typeparam>
        /// <param name="handlerFunction">The function to handle the event</param>
        /// <returns></returns>
        public NotifierBuilder WithEventHandler<TEventData>(Func<DomainEvent<TEventData>, IOperation> handlerFunction)
            => WithEventHandler<TEventData, LambdaHandler<TEventData>>(new LambdaHandler<TEventData>(handlerFunction));

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DomainEventNotifier Build(IResolverContract resolver)
        {
            ValidateBuilder();

            if (resolver == null)
                throw new ArgumentNullException(nameof(resolver));

            return _notifier = new DomainEventNotifier(_options, MergeIntoLazyFactories(resolver));
        }

        public DomainEventNotifier Instance() => _notifier ?? throw new InvalidOperationException($"{typeof(DomainEventNotifier)} is not yet built");

        public Type[] EventTypes() => Enumerable
            .Concat(_handlerInstanceMap.Keys, _handlerTypeMap.Keys)
            .Distinct()
            .ToArray();

        public Type[] HandlerTypesFor(Type eventType) => _handlerTypeMap.TryGetValue(eventType, out var handlerTypes)
            ? handlerTypes.ToArray()
            : Array.Empty<Type>();

        public IHandler[] HandlerInstancesFor(Type eventType) => _handlerInstanceMap.TryGetValue(eventType, out var instance)
            ? instance.ToArray()
            : Array.Empty<IHandler>();

        private IEnumerable<(Type, List<Lazy<IHandler>>)> MergeIntoLazyFactories(IResolverContract resolver)
        {
            var LazyFactory = ToLazyFactory(resolver);
            return Enumerable
                .Concat(_handlerTypeMap.Keys, _handlerInstanceMap.Keys)
                .Select(eventDataType =>
                {
                    var lazyMap = (eventDataType, lazyList: new List<Lazy<IHandler>>());

                    // populate instances
                    if (_handlerInstanceMap.ContainsKey(eventDataType))
                        lazyMap.lazyList.AddRange(_handlerInstanceMap[eventDataType].Select(LazyInstance));

                    // populate types
                    if (_handlerTypeMap.ContainsKey(eventDataType))
                        lazyMap.lazyList.AddRange(_handlerTypeMap[eventDataType].Select(LazyFactory));

                    return lazyMap;
                });
        }

        private Lazy<IHandler> LazyInstance(IHandler instance) => new Lazy<IHandler>(instance);

        private Func<Type, Lazy<IHandler>> ToLazyFactory(IResolverContract resolver)
        {
            return handlerType => new Lazy<IHandler>(() => resolver.Resolve(handlerType).As<IHandler>());
        }

        private void ValidateBuilder()
        {
            if (_notifier != null)
                throw new InvalidOperationException("Notifier had already been built");
        }


        internal class LambdaHandler<TEventData> : IEventHandler<TEventData>
        {
            private readonly Func<DomainEvent<TEventData>, IOperation> _handler;

            internal LambdaHandler(Func<DomainEvent<TEventData>, IOperation> handler)
            {
                _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            }

            public bool CanHandle(DomainEvent<TEventData> @event) => true;

            public IOperation HandleEvent(DomainEvent<TEventData> @event) => _handler.Invoke(@event);
        }
    }
}
