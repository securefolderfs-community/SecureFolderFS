using System;

namespace SecureFolderFS.AvaloniaUI.Events
{
    internal sealed class GenericEventArgs<T> : EventArgs
    {
        public T Value { get; }

        public GenericEventArgs(T value)
        {
            Value = value;
        }
    }
}