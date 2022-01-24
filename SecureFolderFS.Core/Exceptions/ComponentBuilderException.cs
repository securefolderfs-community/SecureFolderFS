using System;

namespace SecureFolderFS.Core.Exceptions
{
    internal sealed class ComponentBuilderException : Exception
    {
        public static readonly ComponentBuilderException AlreadyBuiltException = new("The component builder has been already built.");

        public static readonly ComponentBuilderException ArgumentNotProvidedException = new("An argument wasn't provided to the component builder.");

        private ComponentBuilderException(string message)
            : base(message)
        {
        }
    }
}
