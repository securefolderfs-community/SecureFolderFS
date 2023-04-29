using SecureFolderFS.Shared.Utils;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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
        /// Tries to get the stream that holds the keystore data which can be deserialized using <see cref="KeystoreSerializer"/>.
        /// </summary>
        /// <param name="access">Represents the access to give to the keystore stream.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IResult{T}"/> of <see cref="Stream"/> that represents the action.</returns>
        Task<IResult<Stream?>> GetKeystoreStreamAsync(FileAccess access = FileAccess.Read, CancellationToken cancellationToken = default);
    }
}
