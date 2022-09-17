using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;
using System.Collections.Generic;

namespace SecureFolderFS.Core.FileSystem.CryptFiles
{
    /// <inheritdoc cref="IOpenCryptFileManager"/>
    public abstract class BaseOpenCryptFileManager : IOpenCryptFileManager
    {
        protected readonly Dictionary<string, IOpenCryptFile> openCryptFiles;

        protected BaseOpenCryptFileManager()
        {
            openCryptFiles = new();
        }

        /// <inheritdoc/>
        public virtual IOpenCryptFile? TryGet(string ciphertextPath)
        {
            openCryptFiles.TryGetValue(ciphertextPath, out var openCryptFile);
            return openCryptFile;
        }

        /// <inheritdoc/>
        public virtual IOpenCryptFile CreateNew(string ciphertextPath, BufferHolder headerBuffer)
        {
            var openCryptFile = GetOpenCryptFile(headerBuffer);
            openCryptFiles[ciphertextPath] = openCryptFile;

            return openCryptFile;
        }

        protected abstract IOpenCryptFile GetOpenCryptFile(BufferHolder headerBuffer);

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            openCryptFiles.Values.DisposeCollection();
            openCryptFiles.Clear();
        }
    }
}
