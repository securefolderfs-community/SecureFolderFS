using System;

namespace SecureFolderFS.Core.Exceptions
{
    public sealed class UndefinedCipherSchemeException : Exception
    {
        public UndefinedCipherSchemeException(string cipherSchemeName)
            : base($"{cipherSchemeName} scheme was undefined.")
        {
        }
    }
}
