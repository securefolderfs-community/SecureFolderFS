using SecureFolderFS.Core;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.Vault;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Utilities;
using SecureFolderFS.UI.AppModels;
using SecureFolderFS.UI.ServiceImplementation.Vault;
using System;
using System.Collections.Generic;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultService"/>
    public sealed class VaultService : IVaultService
    {
        /// <inheritdoc/>
        public IVaultCreator VaultCreator { get; } = new VaultCreator();

        /// <inheritdoc/>
        public IVaultUnlocker VaultUnlocker { get; } = new VaultUnlocker();

        /// <inheritdoc/>
        public IVaultAuthenticator VaultAuthenticator { get; } = new VaultAuthenticator();

        /// <inheritdoc/>
        public bool IsNameReserved(string? name)
        {
            return name is not null && (
                   name.Equals(Core.Constants.Vault.Names.VAULT_KEYSTORE_FILENAME, StringComparison.OrdinalIgnoreCase) ||
                   name.Equals(Core.Constants.Vault.Names.VAULT_CONFIGURATION_FILENAME, StringComparison.OrdinalIgnoreCase) ||
                   name.Equals(Core.Constants.Vault.Names.VAULT_CONTENT_FOLDERNAME, StringComparison.OrdinalIgnoreCase));
        }

        /// <inheritdoc/>
        public IAsyncValidator<IFolder> GetVaultValidator()
        {
            return VaultHelpers.NewVaultValidator(StreamSerializer.Instance);
        }

        /// <inheritdoc/>
        public IEnumerable<IFileSystemInfoModel> GetFileSystems()
        {
            yield return new DokanyFileSystemDescriptor();
            yield return new FuseFileSystemDescriptor();
            yield return new WebDavFileSystemDescriptor();
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetContentCiphers()
        {
            yield return Core.Cryptography.Constants.CipherId.XCHACHA20_POLY1305;
            yield return Core.Cryptography.Constants.CipherId.AES_GCM;
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetFileNameCiphers()
        {
            yield return Core.Cryptography.Constants.CipherId.AES_SIV;
            yield return Core.Cryptography.Constants.CipherId.NONE;
        }
    }
}
