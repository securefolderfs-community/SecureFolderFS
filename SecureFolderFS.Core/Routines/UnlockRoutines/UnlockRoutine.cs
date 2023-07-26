using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.ComponentBuilders;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Dokany;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FUSE;
using SecureFolderFS.Core.Models;
using SecureFolderFS.Core.Validators;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Core.WebDav;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Core.Routines.UnlockRoutines
{
    /// <inheritdoc cref="IUnlockRoutine"/>
    internal sealed class UnlockRoutine : IUnlockRoutine
    {
        private readonly IFolder _vaultFolder;
        private readonly IVaultReader _vaultReader;
        private readonly CipherProvider _cipherProvider;
        private VaultConfigurationDataModel? _configDataModel;
        private VaultKeystoreDataModel? _keystoreDataModel;
        private SecretKey? _encKey;
        private SecretKey? _macKey;

        public UnlockRoutine(IFolder vaultFolder, IVaultReader vaultReader)
        {
            _vaultFolder = vaultFolder;
            _vaultReader = vaultReader;
            _cipherProvider = CipherProvider.CreateNew();
        }

        /// <inheritdoc/>
        public string? ContentCipherId { get; private set; }

        /// <inheritdoc/>
        public string? FileNameCipherId { get; private set; }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken)
        {
            var (keystore, config) = await _vaultReader.ReadAsync(cancellationToken);
            _keystoreDataModel = keystore;
            _configDataModel = config;
        }

        /// <inheritdoc/>
        public IUnlockRoutine SetPassword(IPassword password, SecretKey? magic)
        {
            ArgumentNullException.ThrowIfNull(_configDataModel);
            ArgumentNullException.ThrowIfNull(_keystoreDataModel);

            var (encKey, macKey) = VaultParser.DeriveKeystore(password, magic, _keystoreDataModel, _cipherProvider);
            _encKey = encKey;
            _macKey = macKey;

            return this;
        }

        /// <inheritdoc/>
        public Task<IDisposable> FinalizeAsync(CancellationToken cancellationToken)
        {

        }

        /// <inheritdoc/>
        public async Task<IMountableFileSystem> PrepareAndUnlockAsync(FileSystemOptions fileSystemOptions, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(_configDataModel);
            ArgumentNullException.ThrowIfNull(_vaultFolder);
            ArgumentNullException.ThrowIfNull(_macKey);
            ArgumentNullException.ThrowIfNull(_encKey);

            // Create MAC key copy for the validator that can be disposed here
            using var macKeyCopy = _macKey.CreateCopy();

            // Check if the payload has not been tampered with
            var validator = new ConfigurationValidator(_cipherProvider, macKeyCopy);
            var validationResult = await validator.ValidateAsync(_configDataModel, cancellationToken);
            if (!validationResult.Successful)
                throw validationResult.Exception ?? throw new CryptographicException();

            // Build the file system mountable
            var componentBuilder = new FileSystemComponentBuilder()
            {
                ConfigDataModel = _configDataModel,
                ContentFolder = _contentFolder,
                FileSystemOptions = fileSystemOptions
            };

            var (security, directoryIdAccess, pathConverter, streamsAccess) = componentBuilder.BuildComponents(_encKey, _macKey);
            var mountable = fileSystemOptions.AdapterType switch
            {
                FileSystemAdapterType.DokanAdapter => DokanyMountable.CreateMountable(_vaultFolder.Name, _contentFolder, security, directoryIdAccess, pathConverter, streamsAccess, fileSystemOptions.HealthStatistics),
                FileSystemAdapterType.FuseAdapter => FuseMountable.CreateMountable(_vaultFolder.Name, pathConverter, _contentFolder, security, directoryIdAccess, streamsAccess),
                FileSystemAdapterType.WebDavAdapter => WebDavMountable.CreateMountable(_storageService, _vaultFolder.Name, _contentFolder, security, directoryIdAccess, pathConverter, streamsAccess),
                _ => throw new ArgumentOutOfRangeException(nameof(fileSystemOptions.AdapterType))
            };

            return mountable;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _encKey?.Dispose();
            _macKey?.Dispose();
        }
    }

    internal sealed class UnlockFinalization : IDisposable
    {


        /// <inheritdoc/>
        public void Dispose()
        {
            
        }
    }
}
