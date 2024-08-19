using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.UI.ViewModels;
using SecureFolderFS.Uno.AppModels;
using SecureFolderFS.Uno.Platforms.MacCatalyst.AppModels;

namespace SecureFolderFS.Uno.Platforms.MacCatalyst.ServiceImplementation
{
    /// <inheritdoc cref="IVaultService"/>
    internal sealed class MacOsVaultService : BaseVaultService
    {
        /// <inheritdoc/>
        public override IEnumerable<IFileSystemInfoModel> GetFileSystems()
        {
            yield return new MacOsFuseDescriptor();
            yield return new WebDavDescriptor();
        }

        /// <inheritdoc/>
        public override async IAsyncEnumerable<AuthenticationViewModel> GetLoginAsync(IFolder vaultFolder, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var vaultReader = new VaultReader(vaultFolder, StreamSerializer.Instance);
            var config = await vaultReader.ReadConfigurationAsync(cancellationToken);
            var authenticationMethods = config.AuthenticationMethod.Split(Core.Constants.Vault.AuthenticationMethods.SEPARATOR);

            foreach (var item in authenticationMethods)
            {
                yield return item switch
                {
                    Core.Constants.Vault.AuthenticationMethods.AUTH_PASSWORD => new PasswordLoginViewModel(Core.Constants.Vault.AuthenticationMethods.AUTH_PASSWORD),
                    Core.Constants.Vault.AuthenticationMethods.AUTH_KEYFILE => new KeyFileLoginViewModel(Core.Constants.Vault.AuthenticationMethods.AUTH_KEYFILE, vaultFolder),
                    _ => throw new NotSupportedException($"The authentication method '{item}' is not supported by the platform.")
                };
            }
        }

        /// <inheritdoc/>
        public override async IAsyncEnumerable<AuthenticationViewModel> GetCreationAsync(IFolder vaultFolder, string vaultId,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Password
            yield return new PasswordCreationViewModel(Core.Constants.Vault.AuthenticationMethods.AUTH_PASSWORD);

            // Key File
            yield return new KeyFileCreationViewModel(vaultId, Core.Constants.Vault.AuthenticationMethods.AUTH_KEYFILE);

            await Task.CompletedTask;
        }
    }
}
