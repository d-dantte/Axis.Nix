using Axis.Luna.Operation;
using System;

namespace Axis.Nix.Event
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDomainEvent
    {
        /// <summary>
        /// Event name, if needed.
        /// </summary>
        string Name { get; }
    }
}
