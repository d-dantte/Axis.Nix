using Axis.Luna.Operation;

namespace Axis.Nix
{
    public interface IProxyInterceptor
    {
        IOperation<object> Intercept(InvocationContext context);
    }
}
