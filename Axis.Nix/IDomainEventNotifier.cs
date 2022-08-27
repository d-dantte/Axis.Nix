using Axis.Luna.Operation;
using Axis.Nix.Event;

namespace Axis.Nix
{
    /// <summary>
    /// Why do i ned this interface if there is only ever one implementation?
    /// </summary>
    public interface IDomainEventNotifier
    {
        /// <summary>
        /// Notify listeners of the event.
        /// <para>
        /// This method guarantees that all handlers will get a chance to handle the event. Any subsequent errors are consumed as an aggregation of all exceptions.
        /// </para>
        /// </summary>
        /// <typeparam name="TEvent">The event type</typeparam>
        /// <param name="event">The event instance</param>
        /// <returns><see cref="Operation"/> encapsulating the notification of handlers</returns>
        Operation Notify<TEvent>(TEvent @event) where TEvent: IDomainEvent;
    }
}
