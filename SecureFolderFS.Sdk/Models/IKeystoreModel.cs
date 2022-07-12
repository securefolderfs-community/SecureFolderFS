using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Represents a keystore that is accessed via <see cref="Stream"/>.
    /// </summary>
    public interface IKeystoreModel : IDisposable
    {
        /// <summary>
        /// Gets the associated <see cref="IAsyncSerializer{TSerialized}"/> used to deserialize the keystore.
        /// </summary>
        IAsyncSerializer<Stream> KeystoreSerializer { get; }

        /// <summary>
        /// Tries to get the stream that holds the keystore data.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns <see cref="Stream"/> that can be deserialized using <see cref="KeystoreSerializer"/>, otherwise null.</returns>
        Task<Stream?> GetKeystoreStreamAsync(CancellationToken cancellationToken = default);
    }
}
