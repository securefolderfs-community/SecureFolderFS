using System;

namespace SecureFolderFS.Core.Exceptions
{
    internal sealed class UnavailableStreamException : Exception
    {
        public UnavailableStreamException()
            : base("No available stream exists for use.")
        {
        }
    }
}
