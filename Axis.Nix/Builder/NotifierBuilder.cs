using Axis.Luna.Extensions;
using Axis.Nix.Configuration;
using Axis.Nix.Event;
using Axis.Nix.Handler;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Nix.Builder
{
    public sealed class NotifierBuilder
    {

        private readonly Dictionary<Type, List<IHandler>> handlerMaps = new Dictionary<Type, List<IHandler>>();
        private Options? options;

        private NotifierBuilder()
        { }

        public static NotifierBuilder Create() => new NotifierBuilder();

        public Options? BuilderOptions() => options;

        public NotifierBuilder Configure(Options options)
        {
            this.options = options;

            return this;
        }

        public NotifierBuilder WithHandler<THandler>()
        where THandler: IHandler, new()
        {
            var handlerType = ValidateType<THandler>();
            var eventType = handlerType.TryGetGenericInterface(typeof(IEventHandler<>), out var @interface) 
                ? @interface.GetGenericArguments()[0] 
                : throw new ArgumentException($"Invalid handler type: {typeof(THandler)}");

            handlerMaps
                .GetOrAdd(eventType, key => new List<IHandler>())
                .Add(new THandler());

            return this;
        }

        public NotifierBuilder WithHandler<TEvent>(IEventHandler<TEvent> handlerInstance)
        where TEvent : IDomainEvent
        {
            if (handlerInstance == null)
                throw new ArgumentNullException(nameof(handlerInstance));

            var eventType = typeof(TEvent);
            handlerMaps
                .GetOrAdd(eventType, key => new List<IHandler>())
                .Add(handlerInstance);

            return this;
        }


        public DomainEventNotifier Build()
        {
            return new DomainEventNotifier(
                handlers: handlerMaps
                    .ThrowIf(IsInvalidMap, new ArgumentException("Handler Map is invalid"))
                    .Select(map => (map.Key, map.Value)),
                options: options
                    .ThrowIfNull(new ArgumentNullException(nameof(options)))
                    .ThrowIfDefault(new ArgumentException($"Invalid coniguration options")).Value);
        }

        internal Dictionary<Type, List<IHandler>> HandlerMaps() => handlerMaps;

        private Type ValidateType<THandler>()
        where THandler : IHandler => ValidateType(typeof(THandler));

        private Type ValidateType(Type type)
        {
            if (type?.ImplementsGenericInterface(typeof(IEventHandler<>)) == true)
                return type;

            throw new Exception($"Invalid type: {type}");
        }

        private static bool IsInvalidMap(Dictionary<Type, List<IHandler>> handlerMaps)
        {
            if (handlerMaps == null)
                return true;

            // other tests can come here

            return false;
        }
    }
}
