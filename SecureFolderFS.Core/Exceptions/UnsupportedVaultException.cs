using System;

namespace SecureFolderFS.Core.Exceptions
{
    public sealed class UnsupportedVaultException : Exception
    {
        public UnsupportedVaultException(int vaultVersion)
            : base($"The vault version '{vaultVersion}' is not supported by SecureFolderFS.")
        {
        }

        public UnsupportedVaultException(int vaultVersion, string objectName)
            : base($"The vault version '{vaultVersion}' for '{objectName}' factory is not supported by SecureFolderFS.")
        {
        }
    }
}
