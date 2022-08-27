using System;

namespace Axis.Nix.Exceptions
{
    public class InvalidConfigurationException: Exception
    {
        public InvalidConfigurationException(string message)
        :base(message)
        {
        }
    }
}
