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
        public virtual ICryptFile? TryGet(string id)
        {
            lock (openCryptFiles)
            {
                openCryptFiles.TryGetValue(id, out var openCryptFile);
                return openCryptFile;
            }
        }

        /// <inheritdoc/>
        public virtual ICryptFile CreateNew(string id, BufferHolder headerBuffer)
        {
            var cryptFile = GetCryptFile(id, headerBuffer);

            lock (openCryptFiles)
                openCryptFiles[id] = cryptFile;
            
            return cryptFile;
        }

        protected abstract ICryptFile GetCryptFile(string id, BufferHolder headerBuffer);

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
