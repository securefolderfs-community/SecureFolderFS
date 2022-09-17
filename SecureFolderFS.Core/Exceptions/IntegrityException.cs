using System;

namespace SecureFolderFS.Core.Exceptions
{
    [Obsolete("This class should no longer be used. Use CryptographicException instead.")]
    public abstract class IntegrityException : Exception
    {
        protected internal IntegrityException(string message) :
            base(message)
        {
        }
    }
}
