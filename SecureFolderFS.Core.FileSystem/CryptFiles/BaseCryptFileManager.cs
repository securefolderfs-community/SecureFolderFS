using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;
using System.Collections.Generic;

namespace SecureFolderFS.Core.FileSystem.CryptFiles
{
    /// <inheritdoc cref="ICryptFileManager"/>
    public abstract class BaseCryptFileManager : ICryptFileManager
    {
        protected readonly Dictionary<string, ICryptFile> openCryptFiles;

        protected BaseCryptFileManager()
        {
            openCryptFiles = new();
        }

        /// <inheritdoc/>
        public virtual ICryptFile? TryGet(string ciphertextPath)
        {
            lock (openCryptFiles)
            {
                openCryptFiles.TryGetValue(ciphertextPath, out var openCryptFile);
                return openCryptFile;
            }
        }

        /// <inheritdoc/>
        public virtual ICryptFile? CreateNew(string ciphertextPath, BufferHolder headerBuffer)
        {
            var cryptFile = GetCryptFile(ciphertextPath, headerBuffer);
            if (cryptFile is null)
                return null;

            lock (openCryptFiles)
            {
                openCryptFiles[ciphertextPath] = cryptFile;
                return cryptFile;
            }
        }

        protected abstract ICryptFile? GetCryptFile(string ciphertextPath, BufferHolder headerBuffer);

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            lock (openCryptFiles)
            {
                openCryptFiles.Values.DisposeCollection();
                openCryptFiles.Clear();
            }
        }
    }
}
