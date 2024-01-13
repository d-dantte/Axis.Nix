using Axis.Proteus.Interception;
using Axis.Proteus.IoC;
using System;
using System.Linq;

namespace Axis.Nix
{
    internal static class Extensions
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

        /// <summary>
        /// Adds a new registration to the <see cref="IRegistrarContract"/>. If another registration already exists, return false, otherwise, retur true.
        /// </summary>
        /// <typeparam name="TServiceImpl">The service implementation type to register</typeparam>
        /// <param name="contract">The registry contract</param>
        /// <param name="scope">The scope</param>
        /// <param name="profile">The interception profile</param>
        public static bool AddRegistration<TServiceImpl>(this
            IRegistrarContract contract,
            RegistryScope scope = default,
            InterceptorProfile profile = default)
            where TServiceImpl : class
        {
            if (contract.RootManifest().ContainsKey(typeof(TServiceImpl)))
                return false;

            _ = contract.Register<TServiceImpl>(scope, profile);
            return true;
        }
    }
}
