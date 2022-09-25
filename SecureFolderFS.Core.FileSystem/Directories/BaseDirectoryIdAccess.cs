using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem.Directories
{
    /// <inheritdoc cref="IDirectoryIdAccess"/>
    public abstract class BaseDirectoryIdAccess : IDirectoryIdAccess
    {
        /// <inheritdoc/>
        public virtual DirectoryId? GetDirectoryId(string ciphertextPath)
        {
            // Open stream to directory id
            using var stream = OpenDirectoryIdStream(ciphertextPath, FileAccess.Read);
            if (stream is null)
                return null; // TODO: Report that the folder metadata file does not exist to the HealthAPI

            // Read directory id from stream
            var idBuffer = new byte[Constants.DIRECTORY_ID_SIZE];
            var read = stream.Read(idBuffer);
            if (read < Constants.DIRECTORY_ID_SIZE)
                return null;

            return new(idBuffer);
        }

        /// <inheritdoc/>
        public virtual bool SetDirectoryId(string ciphertextPath, DirectoryId directoryId)
        {
            // Open stream to directory id
            using var stream = OpenDirectoryIdStream(ciphertextPath, FileAccess.ReadWrite);
            if (stream is null)
                return false; // TODO: Report that the folder metadata file does not exist to the HealthAPI

            // Write directory id to stream
            stream.Write(directoryId);

            return true;
        }

        /// <inheritdoc/>
        public virtual async Task<DirectoryId?> GetDirectoryIdAsync(string ciphertextPath, CancellationToken cancellationToken = default)
        {
            // Open stream to directory id
            await using var stream = await OpenDirectoryIdStreamAsync(ciphertextPath, FileAccess.Read, cancellationToken);
            if (stream is null)
                return null; // TODO: Report that the folder metadata file does not exist to the HealthAPI

            // Read directory id from stream
            var idBuffer = new byte[Constants.DIRECTORY_ID_SIZE];
            var read = await stream.ReadAsync(idBuffer, cancellationToken);
            if (read < Constants.DIRECTORY_ID_SIZE)
                return null;

            return new(idBuffer);
        }

        /// <inheritdoc/>
        public virtual async Task<bool> SetDirectoryIdAsync(string ciphertextPath, DirectoryId directoryId, CancellationToken cancellationToken = default)
        {
            // Open stream to directory id
            await using var stream = await OpenDirectoryIdStreamAsync(ciphertextPath, FileAccess.ReadWrite, cancellationToken);
            if (stream is null)
                return false; // TODO: Report that the folder metadata file does not exist to the HealthAPI

            // Write directory id to stream
            await stream.WriteAsync(directoryId, cancellationToken);

            return true;
        }

        /// <inheritdoc/>
        public virtual void RemoveDirectoryId(string ciphertextPath)
        {
        }

        protected abstract Stream? OpenDirectoryIdStream(string ciphertextPath, FileAccess access);

        protected abstract Task<Stream?> OpenDirectoryIdStreamAsync(string ciphertextPath, FileAccess access, CancellationToken cancellationToken);
    }
}
