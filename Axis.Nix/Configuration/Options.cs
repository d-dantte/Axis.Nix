using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Axis.Luna.Extensions;
using Axis.Nix.Exceptions;

namespace Axis.Nix.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public readonly struct Options
    {
        /// <summary>
        /// 
        /// </summary>
        public AsyncOptions? AsyncBehavior { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsAsyncBehaviorEnabled => AsyncBehavior != null;


        public Options(AsyncOptions? notifyBehavior)
        {
            AsyncBehavior = notifyBehavior?.ThrowIfDefault(new ArgumentException($"Invalid {nameof(notifyBehavior)}"));
        }
    }

    /// <summary> 
    /// Represents options for asynchronious processing of event notifications.
    /// <para>
    /// When Asycnhronious behavior is specified, Event notification enforces a "fire and forget" model - This works because it is used for the
    /// Event notification process(es) that do not return value(s).
    /// </para>
    /// </summary>
    public readonly struct AsyncOptions
    {
        #region static
        /// <summary>
        /// Provider function for the default task scheduler
        /// </summary>
        public static Func<TaskScheduler> DefaultSchedulerProvider { get; } = () => TaskScheduler.Default;

        /// <summary>
        /// Creates a <see cref="AsyncOptions"/> instance using default values
        /// </summary>
        public static AsyncOptions DefaultOptions() => new AsyncOptions(DefaultSchedulerProvider, null);
        #endregion

        /// <summary>
        /// A list of actions invoked on errors that occur when event notification is handled asynchroniously.
        /// </summary>
        public Action<HandlerException[]>[] ErrorConsumers { get; }

        /// <summary>
        /// A sink function that receives the task in which the event is handled asynchroniously.
        /// This makes it possible to react to the different completion <see cref="TaskStatus"/>es of the task.
        /// <para>
        /// Note that:
        /// <list type="number">
        ///     <item>If the event handlers all succeeded, the task status will be <see cref="TaskStatus.RanToCompletion"/>.</item>
        ///     <item>
        ///         If some event handlers errored, and there are no Error Consumers registered on the <see cref="AsyncOptions"/>, 
        ///         the task will be <see cref="TaskStatus.Faulted"/> with an <see cref="AggregateException"/> containing all the 
        ///         resultant <see cref="HandlerException"/>.
        ///     </item>
        ///     <item>
        ///       If on the other hand there are Error Consumers registered on the <see cref="AsyncOptions"/>, these all get called, and if all succeed,
        ///       the task status will be <see cref="TaskStatus.RanToCompletion"/>.
        ///     </item>
        ///     <item>
        ///       If any of the error consumers should fail, their exceptions are collected in yet another <see cref="AggregateException"/>, and this is
        ///       in turn thrown from the task - meaning the task is  <see cref="TaskStatus.Faulted"/>.
        ///     </item>
        /// </list>
        /// </para>
        /// </summary>
        public Action<Task> TaskSink { get; }

        /// <summary>
        /// Provider function for the <see cref="TaskScheduler"/>. This method is only ever called once, and the returned value is cached and used
        /// to create all subsequent tasks.
        /// </summary>
        public Func<TaskScheduler> SchedulerProvider { get; }

        /// <summary>
        /// <see cref="CancellationToken"/> is given to all <see cref="Task"/> instances created by the scheduler
        /// </summary>
        public CancellationToken? CancellationToken { get; }

        /// <summary>
        /// <see cref="TaskCreationOptions"/> that is used to create all <see cref="Task"/> instances.
        /// </summary>
        public TaskCreationOptions TaskCreationOptions { get; }

        public AsyncOptions(
            Func<TaskScheduler> schedulerProvider,
            Action<Task> taskSink,
            CancellationToken? cancellationToken = null,
            TaskCreationOptions taskCreationOptions = TaskCreationOptions.None,
            params Action<HandlerException[]>[] errorConsumers)
        {
            SchedulerProvider = schedulerProvider ?? throw new ArgumentNullException(nameof(schedulerProvider));
            CancellationToken = cancellationToken;
            TaskCreationOptions = taskCreationOptions;
            TaskSink = taskSink;

            ErrorConsumers = errorConsumers?
                .Where(consumer => consumer != null)
                .ToArray()
                ?? Array.Empty<Action<HandlerException[]>>();
        }

        public AsyncOptions(
            Action<Task> taskSink,
            CancellationToken? cancellationToken = null,
            TaskCreationOptions taskCreationOptions = TaskCreationOptions.None,
            params Action<HandlerException[]>[] errorConsumers)
            : this(DefaultSchedulerProvider, taskSink, cancellationToken, taskCreationOptions, errorConsumers)
        {
        }

        public AsyncOptions(
            CancellationToken? cancellationToken = null,
            TaskCreationOptions taskCreationOptions = TaskCreationOptions.None,
            params Action<HandlerException[]>[] errorConsumers)
            : this(DefaultSchedulerProvider, null, cancellationToken, taskCreationOptions, errorConsumers)
        {
        }
    }
}
