using SecureFolderFS.Shared.Utils;
using System.IO;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// A service that manages and exposes serialization members.
    /// </summary>
    public interface ISerializationService
    {
        /// <summary>
        /// Gets a new instance of <see cref="IAsyncSerializer{TSerialized}"/> of <see cref="Stream"/>.
        /// </summary>
        IAsyncSerializer<Stream> StreamSerializer { get; }

        /// <summary>
        /// Gets a new instance of <see cref="IAsyncSerializer{TSerialized}"/> of <see cref="byte"/> array.
        /// </summary>
        IAsyncSerializer<byte[]> BufferSerializer { get; }

        /// <summary>
        /// Gets a new generic instance of <see cref="IAsyncSerializer{TSerialized}"/> of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type used in serialization.</typeparam>
        /// <returns>A new instance of <see cref="IAsyncSerializer{TSerialized}"/>.</returns>
        IAsyncSerializer<T> GetGenericSerializer<T>();
    }
}
