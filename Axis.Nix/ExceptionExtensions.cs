using Axis.Luna.Extensions;

namespace Axis.Nix
{
    public static class ExceptionExtensions
    {
        public static T? ThrowIfDefault<T>(this T? value, System.Exception ex) where T : struct
        {
            if (default(T?).Equals(value.Value))
                return ex.Throw<T?>();

            return value;
        }
    }
}
