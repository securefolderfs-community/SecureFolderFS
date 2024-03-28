using System;
using System.Collections.Generic;
using OwlCore.Storage;
using SecureFolderFS.Core.Validators;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.AppModels;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultService"/>
    public sealed class VaultService : IVaultService
    {
        /// <inheritdoc/>
        public IAsyncValidator<IFolder> VaultValidator { get; } = new VaultValidator(StreamSerializer.Instance);

        /// <inheritdoc/>
        public int LatestVaultVersion { get; } = Core.Constants.Vault.Versions.LATEST_VERSION;

        /// <inheritdoc/>
        public bool IsNameReserved(string? name)
        {
            return name is not null && (
                   name.Equals(Core.Constants.Vault.Names.VAULT_KEYSTORE_FILENAME, StringComparison.OrdinalIgnoreCase) ||
                   name.Equals(Core.Constants.Vault.Names.VAULT_CONFIGURATION_FILENAME, StringComparison.OrdinalIgnoreCase) ||
                   name.Equals(Core.Constants.Vault.Names.VAULT_CONTENT_FOLDERNAME, StringComparison.OrdinalIgnoreCase));
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
