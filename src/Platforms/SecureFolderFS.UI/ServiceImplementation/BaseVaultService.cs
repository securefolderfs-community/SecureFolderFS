using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.Validators;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultService"/>
    public abstract class BaseVaultService : IVaultService
    {
        /// <inheritdoc/>
        public int LatestVaultVersion { get; } = Core.Constants.Vault.Versions.LATEST_VERSION;

        /// <inheritdoc/>
        public IAsyncValidator<IFolder> VaultValidator { get; } = new VaultValidator(StreamSerializer.Instance);

        /// <inheritdoc/>
        public virtual bool IsNameReserved(string? name)
        {
            return name is not null && (
                   name.Equals(Core.Constants.Vault.Names.VAULT_KEYSTORE_FILENAME, StringComparison.OrdinalIgnoreCase) ||
                   name.Equals(Core.Constants.Vault.Names.VAULT_CONFIGURATION_FILENAME, StringComparison.OrdinalIgnoreCase) ||
                   name.Equals(Core.Constants.Vault.Names.VAULT_CONTENT_FOLDERNAME, StringComparison.OrdinalIgnoreCase));
        }

        /// <inheritdoc/>
        public virtual IEnumerable<string> GetContentCiphers()
        {
            yield return Core.Cryptography.Constants.CipherId.XCHACHA20_POLY1305;
            yield return Core.Cryptography.Constants.CipherId.AES_GCM;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<string> GetFileNameCiphers()
        {
            yield return Core.Cryptography.Constants.CipherId.AES_SIV;
            yield return Core.Cryptography.Constants.CipherId.NONE;
        }

        /// <inheritdoc/>
        public virtual async Task<VaultOptions> GetVaultOptionsAsync(IFolder vaultFolder, CancellationToken cancellationToken = default)
        {
            var vaultReader = new VaultReader(vaultFolder, StreamSerializer.Instance);
            var config = await vaultReader.ReadConfigurationAsync(cancellationToken);

            return new()
            {
                AuthenticationMethod = config.AuthenticationMethod.Split(Core.Constants.Vault.Authentication.SEPARATOR),
                ContentCipherId = config.ContentCipherId,
                FileNameCipherId = config.FileNameCipherId,
                VaultId = config.Uid,
                Version = config.Version
            };
        }

        /// <inheritdoc/>
        public abstract IEnumerable<IFileSystemInfoModel> GetFileSystems();

        /// <inheritdoc/>
        public abstract IAsyncEnumerable<AuthenticationViewModel> GetLoginAsync(IFolder vaultFolder, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract IAsyncEnumerable<AuthenticationViewModel> GetCreationAsync(IFolder vaultFolder, string vaultId, CancellationToken cancellationToken = default);
    }
}
