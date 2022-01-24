using System;

namespace SecureFolderFS.Core.Exceptions
{
    public sealed class VaultInitializationException : Exception
    {
        public VaultInitializationException(string message)
            : base(message)
        {
        }
    }
}
