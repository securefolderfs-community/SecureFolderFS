using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.Helpers.Paths;
using SecureFolderFS.Core.Migration;
using SecureFolderFS.Core.Validators;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultService"/>
    public class VaultService : IVaultService
    {
        /// <inheritdoc/>
        public int LatestVaultVersion { get; } = Core.Constants.Vault.Versions.LATEST_VERSION;

        /// <inheritdoc/>
        public string ContentFolderName { get; } = Core.Constants.Vault.Names.VAULT_CONTENT_FOLDERNAME;

        /// <inheritdoc/>
        public IAsyncValidator<IFolder> VaultValidator { get; } = new VaultValidator(StreamSerializer.Instance);

        /// <inheritdoc/>
        public virtual bool IsNameReserved(string? name)
        {
            return name is not null && (
                    PathHelpers.IsCoreName(name) ||
                    name.Equals(Core.Constants.Vault.Names.VAULT_KEYSTORE_FILENAME, StringComparison.OrdinalIgnoreCase) ||
                    name.Equals(Core.Constants.Vault.Names.VAULT_CONFIGURATION_FILENAME, StringComparison.OrdinalIgnoreCase) ||
                    name.Equals(Core.Constants.Vault.Names.VAULT_CONTENT_FOLDERNAME, StringComparison.OrdinalIgnoreCase));
        }

        /// <inheritdoc/>
        public virtual IEnumerable<string> GetEncodingOptions()
        {
            // TODO: (v3) Swap default order when Base4K (Vault V3) is implemented
            yield return Core.Cryptography.Constants.CipherId.ENCODING_BASE64URL;
            yield return Core.Cryptography.Constants.CipherId.ENCODING_BASE4K;
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
                NameEncodingId = config.FileNameEncodingId,
                VaultId = config.Uid,
                Version = config.Version
            };
        }

        /// <inheritdoc/>
        public virtual async Task<IVaultMigratorModel> GetMigratorAsync(IFolder vaultFolder, CancellationToken cancellationToken = default)
        {
            var vaultReader = new VaultReader(vaultFolder, StreamSerializer.Instance);
            var configVersion = await vaultReader.ReadVersionAsync(cancellationToken);

            return configVersion.Version switch
            {
                Core.Constants.Vault.Versions.V1 => Migrators.GetMigratorV1_V2(vaultFolder, StreamSerializer.Instance),
                Core.Constants.Vault.Versions.V2 => Migrators.GetMigratorV2_V3(vaultFolder, StreamSerializer.Instance),
                _ => throw new ArgumentOutOfRangeException(nameof(configVersion.Version))
            };
        }
    }
}
