using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Utils;
using System;
using System.IO;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="ISerializationService"/>
    internal sealed class SerializationService : ISerializationService
    {
        /// <inheritdoc/>
        public IAsyncSerializer<Stream> StreamSerializer { get; } = Serialization.StreamSerializer.Instance;

        /// <inheritdoc/>
        public IAsyncSerializer<byte[]> BufferSerializer { get; } = Serialization.BufferSerializer.Instance;

        /// <inheritdoc/>
        public IAsyncSerializer<T> GetGenericSerializer<T>()
        {
            throw new NotSupportedException(); // TODO: Impl
        }
    }
}
