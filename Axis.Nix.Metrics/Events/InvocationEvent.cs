using Axis.Luna.Extensions;
using System;
using System.Linq;
using System.Reflection;

namespace Axis.Nix.Metrics.AppMetrics.Events
{
    public interface IInvocationEvent 
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="arguments"></param>
        /// <param name="result"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static IInvocationEvent Of(
            MethodInfo method,
            object[] arguments,
            object result,
            TimeSpan duration)
            => new SuccessfulStaticInvocationEvent(method, arguments, result, duration);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        /// <param name="arguments"></param>
        /// <param name="result"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static IInvocationEvent Of(
            object instance,
            MethodInfo method,
            object[] arguments,
            object result,
            TimeSpan duration)
            => new SuccessfulInstanceInvocationEvent(instance, method, arguments, result, duration);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="arguments"></param>
        /// <param name="error"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static IInvocationEvent Of(
            MethodInfo method,
            object[] arguments,
            Exception error,
            TimeSpan duration)
            => new FaultedStaticInvocationEvent(method, arguments, error, duration);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        /// <param name="arguments"></param>
        /// <param name="error"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static IInvocationEvent Of(
            object instance,
            MethodInfo method,
            object[] arguments,
            Exception error,
            TimeSpan duration)
            => new FaultedInstanceInvocationEvent(instance, method, arguments, error, duration);

        #region Members
        /// <summary>
        /// 
        /// </summary>
        MethodInfo Method { get; }

        /// <summary>
        /// 
        /// </summary>
        object[] Arguments { get; }

        /// <summary>
        /// 
        /// </summary>
        TimeSpan Duration { get; }
        #endregion

        #region Union types

        /// <summary>
        /// 
        /// </summary>
        public readonly struct SuccessfulStaticInvocationEvent : IInvocationEvent, IEquatable<SuccessfulStaticInvocationEvent>
        {
            private readonly object[] _arguments;

            public MethodInfo Method { get; }

            public object[] Arguments => _arguments;

            public object Result { get; }

            public TimeSpan Duration { get; }

            public SuccessfulStaticInvocationEvent(
                MethodInfo method,
                object[] arguments,
                object result,
                TimeSpan duration)
            {
                Method = method ?? throw new ArgumentNullException(nameof(method));
                _arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
                Result = result;
                Duration = duration;
            }

            public bool Equals(SuccessfulStaticInvocationEvent other)
            {
                return other.Method.NullOrEquals(Method)
                    && other.Arguments.NullOrTrue(Arguments, Enumerable.SequenceEqual)
                    && other.Result.NullOrEquals(Result)
                    && other.Duration.Equals(Duration);
            }

            public override bool Equals(object obj)
            {
                return obj is SuccessfulStaticInvocationEvent other
                   && other.Equals(this);
            }

            public override int GetHashCode() => HashCode.Combine(Method, Arguments, Result, Duration);

            public static bool operator ==(SuccessfulStaticInvocationEvent first, SuccessfulStaticInvocationEvent second) => first.Equals(second);
            public static bool operator !=(SuccessfulStaticInvocationEvent first, SuccessfulStaticInvocationEvent second) => !first.Equals(second);
        }

        /// <summary>
        /// 
        /// </summary>
        public readonly struct SuccessfulInstanceInvocationEvent : IInvocationEvent, IEquatable<SuccessfulInstanceInvocationEvent>
        {
            private readonly object[] _arguments;

            public object Instance { get; }

            public MethodInfo Method { get; }

            public object[] Arguments => _arguments;

            public object Result { get; }

            public TimeSpan Duration { get; }

            public SuccessfulInstanceInvocationEvent(
                object instance,
                MethodInfo method,
                object[] arguments,
                object result,
                TimeSpan duration)
            {

                Instance = instance ?? throw new ArgumentNullException(nameof(instance));
                Method = method ?? throw new ArgumentNullException(nameof(method));
                _arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
                Result = result;
                Duration = duration;
            }

            public bool Equals(SuccessfulInstanceInvocationEvent other)
            {
                return other.Method.NullOrEquals(Method)
                    && other.Arguments.NullOrTrue(Arguments, Enumerable.SequenceEqual)
                    && other.Result.NullOrEquals(Result)
                    && other.Duration.Equals(Duration)
                    && other.Instance.NullOrEquals(Instance);
            }

            public override bool Equals(object obj)
            {
                return obj is SuccessfulInstanceInvocationEvent other
                   && other.Equals(this);
            }

            public override int GetHashCode() => HashCode.Combine(Instance, Method, Arguments, Result, Duration);

            public static bool operator ==(SuccessfulInstanceInvocationEvent first, SuccessfulInstanceInvocationEvent second) => first.Equals(second);
            public static bool operator !=(SuccessfulInstanceInvocationEvent first, SuccessfulInstanceInvocationEvent second) => !first.Equals(second);
        }

        /// <summary>
        /// 
        /// </summary>
        public readonly struct FaultedStaticInvocationEvent : IInvocationEvent, IEquatable<FaultedStaticInvocationEvent>
        {
            private readonly object[] _arguments;

            public MethodInfo Method { get; }

            public object[] Arguments => _arguments;

            public Exception Error { get; }

            public TimeSpan Duration { get; }

            public FaultedStaticInvocationEvent(
                MethodInfo method,
                object[] arguments,
                Exception error,
                TimeSpan duration)
            {
                Method = method ?? throw new ArgumentNullException(nameof(method));
                _arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
                Error = error ?? throw new ArgumentNullException(nameof(error));
                Duration = duration;
            }

            public bool Equals(FaultedStaticInvocationEvent other)
            {
                return other.Method.NullOrEquals(Method)
                    && other.Arguments.NullOrTrue(Arguments, Enumerable.SequenceEqual)
                    && other.Error.NullOrEquals(Error)
                    && other.Duration.Equals(Duration);
            }

            public override bool Equals(object obj)
            {
                return obj is FaultedStaticInvocationEvent other
                   && other.Equals(this);
            }

            public override int GetHashCode() => HashCode.Combine(Method, Arguments, Error, Duration);

            public static bool operator ==(FaultedStaticInvocationEvent first, FaultedStaticInvocationEvent second) => first.Equals(second);
            public static bool operator !=(FaultedStaticInvocationEvent first, FaultedStaticInvocationEvent second) => !first.Equals(second);
        }

        /// <summary>
        /// 
        /// </summary>
        public readonly struct FaultedInstanceInvocationEvent : IInvocationEvent, IEquatable<FaultedInstanceInvocationEvent>
        {
            private readonly object[] _arguments;

            public object Instance { get; }

            public MethodInfo Method { get; }

            public object[] Arguments => _arguments;

            public Exception Error { get; }

            public TimeSpan Duration { get; }

            public FaultedInstanceInvocationEvent(
                object instance,
                MethodInfo method,
                object[] arguments,
                Exception error,
                TimeSpan duration)
            {
                Instance = instance ?? throw new ArgumentNullException(nameof(instance));
                Method = method ?? throw new ArgumentNullException(nameof(method));
                _arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
                Error = error ?? throw new ArgumentNullException(nameof(error));
                Duration = duration;
            }

            public bool Equals(FaultedInstanceInvocationEvent other)
            {
                return other.Method.NullOrEquals(Method)
                    && other.Arguments.NullOrTrue(Arguments, Enumerable.SequenceEqual)
                    && other.Error.NullOrEquals(Error)
                    && other.Duration.Equals(Duration)
                    && other.Instance.NullOrEquals(Instance);
            }

            public override bool Equals(object obj)
            {
                return obj is FaultedInstanceInvocationEvent other
                   && other.Equals(this);
            }

            public override int GetHashCode() => HashCode.Combine(Instance, Method, Arguments, Error, Duration);

            public static bool operator ==(FaultedInstanceInvocationEvent first, FaultedInstanceInvocationEvent second) => first.Equals(second);
            public static bool operator !=(FaultedInstanceInvocationEvent first, FaultedInstanceInvocationEvent second) => !first.Equals(second);
        }

        #endregion

    }
}
