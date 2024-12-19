using System;
using OwlCore.Storage;

namespace SecureFolderFS.Storage.StorageProperties
{
    /// <inheritdoc cref="IStorageProperty{T}"/>
    public class GenericProperty<T>(T value) : IStorageProperty<T>
    {
        /// <inheritdoc/>
        public T Value { get; } = value;

        /// <inheritdoc/>
        public event EventHandler<T>? ValueUpdated;

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            (Value as IDisposable)?.Dispose();
        }
    }
}
