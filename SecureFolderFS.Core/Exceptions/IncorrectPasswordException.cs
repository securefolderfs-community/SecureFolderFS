using System;

namespace SecureFolderFS.Core.Exceptions
{
    public sealed class IncorrectPasswordException : Exception
    {
        public IncorrectPasswordException()
            : base("The password is incorrect")
        {
        }
    }
}
