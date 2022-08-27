using System;
using System.Linq;

namespace Axis.Nix
{
    internal static class TypeExtensions
    {
        public static bool TryGetGenericInterface(this Type type, Type genericInterfaceDefinition, out Type genericInterface)
        {
            genericInterface = null;

            if (!genericInterfaceDefinition.IsGenericTypeDefinition)
                return false;

            if (!genericInterfaceDefinition.IsInterface)
                return false;

            genericInterface = type
                .GetInterfaces()
                .Where(_bt => _bt.IsGenericType)
                .Where(_bt => _bt.GetGenericTypeDefinition() == genericInterfaceDefinition)
                .FirstOrDefault();

            return genericInterface != null;
        }
    }
}
