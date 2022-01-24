using System;

namespace SecureFolderFS.Core.Exceptions
{
    public sealed class UnavailableStreamException : Exception
    {
        public UnavailableStreamException()
            : base("No available stream exists for use.")
        {
        }
    }
}
