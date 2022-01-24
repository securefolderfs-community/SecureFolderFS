using System;

namespace SecureFolderFS.Core.Exceptions
{
    public abstract class IntegrityException : Exception
    {
        protected internal IntegrityException(string message) :
            base(message)
        {
        }
    }
}
