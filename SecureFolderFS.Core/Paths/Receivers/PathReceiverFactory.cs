using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.FileNames;
using SecureFolderFS.Core.Paths.DirectoryMetadata;
using SecureFolderFS.Core.VaultDataStore;
using SecureFolderFS.Core.Sdk.Paths;

namespace SecureFolderFS.Core.Paths.Receivers
{
    internal sealed class PathReceiverFactory
    {
        private readonly VaultVersion _vaultVersion;

        private readonly VaultPath _vaultPath;

        private readonly IDirectoryIdReceiver _directoryIdReceiver;

        private readonly IFileNameReceiver _fileNameReceiver;

        private readonly FileNameCipherScheme _fileNameCipherScheme;

        public PathReceiverFactory(VaultVersion vaultVersion,
            VaultPath vaultPath,
            IDirectoryIdReceiver directoryIdReceiver,
            IFileNameReceiver fileNameReceiver,
            FileNameCipherScheme fileNameCipherScheme)
        {
            _vaultVersion = vaultVersion;
            _vaultPath = vaultPath;
            _directoryIdReceiver = directoryIdReceiver;
            _fileNameReceiver = fileNameReceiver;
            _fileNameCipherScheme = fileNameCipherScheme;
        }

        public IPathReceiver GetPathReceiver()
        {
            // Version control can be added
            switch (_fileNameCipherScheme)
            {
                case FileNameCipherScheme.None:
                    return new UnencryptedUniformPathReceiver(_vaultPath);

                case FileNameCipherScheme.AES_SIV:
                    return new EncryptedUniformPathReceiver(_vaultPath, _directoryIdReceiver, _fileNameReceiver);

                default:
                case FileNameCipherScheme.Undefined:
                    throw new UndefinedCipherSchemeException(nameof(FileNameCipherScheme));
            }
        }
    }
}
