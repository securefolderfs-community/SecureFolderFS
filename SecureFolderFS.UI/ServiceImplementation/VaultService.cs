using SecureFolderFS.Core;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.Vault;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Utils;
using SecureFolderFS.UI.AppModels;
using SecureFolderFS.UI.ServiceImplementation.Vault;
using System;
using System.Collections.Generic;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultService"/>
    public sealed class VaultService : IVaultService
    {
        private static Dictionary<string, IFileSystemInfoModel> FileSystems { get; } = new()
        {
            { Core.Constants.FileSystemId.DOKAN_ID, new DokanyFileSystemDescriptor() },
            { Core.Constants.FileSystemId.FUSE_ID, new FuseFileSystemDescriptor() },
            { Core.Constants.FileSystemId.WEBDAV_ID, new WebDavFileSystemDescriptor() }
        };

        /// <inheritdoc/>
        public IVaultCreator VaultCreator { get; } = new VaultCreator();

        /// <inheritdoc/>
        public IVaultUnlocker VaultUnlocker { get; }

        /// <inheritdoc/>
        public IVaultAuthenticator VaultAuthenticator { get; }

        /// <inheritdoc/>
        public bool IsNameReserved(string? name)
        {
            return name is not null && (
                   name.Equals(Core.Constants.Vault.VAULT_KEYSTORE_FILENAME, StringComparison.Ordinal) ||
                   name.Equals(Core.Constants.Vault.VAULT_CONFIGURATION_FILENAME, StringComparison.Ordinal) ||
                   name.Equals(Core.Constants.Vault.VAULT_CONTENT_FOLDERNAME, StringComparison.Ordinal));
        }

        /// <inheritdoc/>
        public IAsyncValidator<IFolder> GetVaultValidator()
        {
            return VaultHelpers.NewVaultValidator(StreamSerializer.Instance);
        }

        /// <inheritdoc/>
        public IEnumerable<IFileSystemInfoModel> GetFileSystems()
        {
            foreach (var item in FileSystems.Values)
            {
                yield return item;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetContentCiphers()
        {
            yield return Core.Constants.CipherId.XCHACHA20_POLY1305;
            yield return Core.Constants.CipherId.AES_GCM;
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetFileNameCiphers()
        {
            yield return Core.Constants.CipherId.AES_SIV;
            yield return Core.Constants.CipherId.NONE;
        }
    }
}
