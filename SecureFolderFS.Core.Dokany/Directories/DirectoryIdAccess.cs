using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.FileSystem.Directories;

namespace SecureFolderFS.Core.Dokany.Directories
{
    /// <inheritdoc cref="IDirectoryIdAccess"/>
    public abstract class DirectoryIdAccess : IDirectoryIdAccess
    {
        /// <inheritdoc/>
        public DirectoryId? GetDirectoryId(string ciphertextPath)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public bool SetDirectoryId(string ciphertextPath, DirectoryId directoryId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<DirectoryId?> GetDirectoryIdAsync(string ciphertextPath, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> SetDirectoryIdAsync(string ciphertextPath, DirectoryId directoryId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void RemoveDirectoryId(string ciphertextPath)
        {
            throw new NotImplementedException();
        }
    }
}
