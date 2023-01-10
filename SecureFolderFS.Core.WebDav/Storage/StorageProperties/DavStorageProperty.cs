using NWebDav.Server.Storage.StorageProperties;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.Storage.StorageProperties
{
    /// <inheritdoc cref="IDavProperty"/>
    internal abstract class DavStorageProperty : IDavProperty
    {
        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public object Value { get; }

        /// <inheritdoc/>
        public event EventHandler<object>? OnValueUpdated;

        protected DavStorageProperty(string name, object value)
        {
            Name = name;
            Value = value;
        }

        /// <inheritdoc/>
        public abstract Task ModifyAsync(object newValue, CancellationToken cancellationToken = default);
    }
}
