using System;

namespace SecureFolderFS.Core.FileSystem.Exceptions
{
    public sealed class UnavailableStreamException : Exception
    {
        public UnavailableStreamException()
            : base("No available streams exist for use.")
        {
        }
    }
}
