using SecureFolderFS.Core.FileSystem.Directories;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Directories
{
    /// <inheritdoc cref="IDirectoryIdAccess"/>
    public abstract class InstantDirectoryIdAccess : IDirectoryIdAccess
    {
        /// <inheritdoc/>
        public virtual DirectoryId? GetDirectoryId(string ciphertextPath)
        {
            // Check if path is empty
            if (ciphertextPath.Length == 0)
                return DirectoryId.Empty;

            // Open stream to directory id
            using var fileStream = OpenDirectoryIdStream(ciphertextPath, FileMode.Open, FileAccess.Read);
            if (fileStream is null)
                return null; // TODO: Report that the folder metadata file does not exist to the HealthAPI

            //if (fileStream.Length < FileSystem.Constants.DIRECTORY_ID_SIZE)
            //    return null; // TODO: Report that the ID size is smaller than requested to the HealthAPI.

            // Read directory id from stream
            var buffer = new byte[FileSystem.Constants.DIRECTORY_ID_SIZE];
            var read = fileStream.Read(buffer);
            if (read < FileSystem.Constants.DIRECTORY_ID_SIZE)
                return null;

            return new(buffer);
        }

        /// <inheritdoc/>
        public virtual bool SetDirectoryId(string ciphertextPath, DirectoryId directoryId)
        {
            // Open stream to directory id
            using var fileStream = OpenDirectoryIdStream(ciphertextPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            if (fileStream is null)
                return false; // TODO: Report that the folder metadata file does not exist to the HealthAPI

            // Write directory id to stream
            fileStream.Write(directoryId);

            return true;
        }

        /// <inheritdoc/>
        public virtual async Task<DirectoryId?> GetDirectoryIdAsync(string ciphertextPath, CancellationToken cancellationToken = default)
        {
            // Check if path is empty
            if (ciphertextPath.Length == 0)
                return DirectoryId.Empty;

            // Open stream to directory id
            await using var fileStream =
                await OpenDirectoryIdStreamAsync(ciphertextPath, FileMode.Open, FileAccess.Read, cancellationToken);
            if (fileStream is null)
                return null; // TODO: Report that the folder metadata file does not exist to the HealthAPI

            //if (fileStream.Length < FileSystem.Constants.DIRECTORY_ID_SIZE)
            //    return null; // TODO: Report that the ID size is smaller than requested to the HealthAPI.

            // Read directory id from stream
            var buffer = new byte[FileSystem.Constants.DIRECTORY_ID_SIZE];
            var read = await fileStream.ReadAsync(buffer, cancellationToken);
            if (read < FileSystem.Constants.DIRECTORY_ID_SIZE)
                return null;

            return new(buffer);
        }

        /// <inheritdoc/>
        public virtual async Task<bool> SetDirectoryIdAsync(string ciphertextPath, DirectoryId directoryId, CancellationToken cancellationToken = default)
        {
            // Open stream to directory id
            await using var fileStream = await OpenDirectoryIdStreamAsync(ciphertextPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, cancellationToken);
            if (fileStream is null)
                return false; // TODO: Report that the folder metadata file does not exist to the HealthAPI

            // Write directory id to stream
            await fileStream.WriteAsync(directoryId, cancellationToken);

            return true;
        }

        /// <inheritdoc/>
        public virtual void RemoveDirectoryId(string ciphertextPath)
        {
        }

        protected abstract Stream? OpenDirectoryIdStream(string ciphertextPath, FileMode mode, FileAccess access);

        protected abstract Task<Stream?> OpenDirectoryIdStreamAsync(string ciphertextPath, FileMode mode, FileAccess access, CancellationToken cancellationToken);
    }
}
