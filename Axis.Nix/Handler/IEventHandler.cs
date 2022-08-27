using Axis.Luna.Operation;
using Axis.Nix.Event;

namespace Axis.Nix.Handler
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface IEventHandler<TEvent> : IHandler
    where TEvent: IDomainEvent
    {
        /// <summary>
        /// Handle the exception
        /// <para>
        /// NOTE: callers of this method SHOULD NEVER have to catch an exception from it - any thrown exception should be 
        /// encapsulated within the <see cref="Operation"/> instance.
        /// </para>
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        Operation HandleEvent(TEvent @event);
    }
}
