using Axis.Luna.Operation;
using Axis.Nix.Event;

namespace Axis.Nix.Handler
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEventData"></typeparam>
    public interface IEventHandler<TEventData>: IHandler
    {
        /// <summary>
        /// Handle the exception
        /// <para>
        /// NOTE: callers of this method SHOULD NEVER have to catch an exception from it - any thrown exception should be 
        /// encapsulated within the <see cref="IOperation"/> instance.
        /// </para>
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        IOperation HandleEvent(DomainEvent<TEventData> @event);

        /// <summary>
        /// Indicates that this handler can handle the given event instance.
        /// <para>
        /// Implementations can access any information on the event to determine their willingness to handle it.
        /// </para>
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        bool CanHandle(DomainEvent<TEventData> @event);
    }
}
