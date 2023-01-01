using SecureFolderFS.Sdk.Storage.StorageProperties;
using System;

namespace SecureFolderFS.Core.WebDav.Http.Storage.StorageProperties
{
    /// <inheritdoc cref="IStorageProperty{T}"/>
    internal sealed class NonObservableStorageProperty<T> : IStorageProperty<T>
    {
        /// <inheritdoc/>
        public T Value { get; }

        /// <inheritdoc/>
        public event EventHandler<T>? ValueUpdated;

        public NonObservableStorageProperty(T value)
        {
            Value = value;
        }
    }
}
