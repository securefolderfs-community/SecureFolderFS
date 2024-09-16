using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using OwlCore.Storage;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.UI.ViewModels;
using SecureFolderFS.Uno.AppModels;
using SecureFolderFS.Uno.Platforms.Windows.AppModels;
using SecureFolderFS.Uno.ViewModels;
using Windows.Security.Credentials;

namespace SecureFolderFS.Uno.Platforms.Windows.ServiceImplementation
{
    /// <inheritdoc cref="IVaultService"/>
    internal sealed class WindowsVaultService : BaseVaultService
    {
        /// <inheritdoc/>
        public override IEnumerable<IFileSystemInfoModel> GetFileSystems()
        {
            yield return new WindowsDokanyDescriptor();
            yield return new WebDavDescriptor();
        }

        /// <inheritdoc/>
        public override async IAsyncEnumerable<AuthenticationViewModel> GetLoginAsync(IFolder vaultFolder, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var vaultReader = new VaultReader(vaultFolder, StreamSerializer.Instance);
            var config = await vaultReader.ReadConfigurationAsync(cancellationToken);
            var authenticationMethods = config.AuthenticationMethod.Split(Core.Constants.Vault.Authentication.SEPARATOR);

            foreach (var item in authenticationMethods)
            {
                yield return item switch
                {
                    // Password
                    Core.Constants.Vault.Authentication.AUTH_PASSWORD => new PasswordLoginViewModel(),

                    // Windows Hello
                    Core.Constants.Vault.Authentication.AUTH_WINDOWS_HELLO => await KeyCredentialManager.IsSupportedAsync().AsTask(cancellationToken)
                            ? new WindowsHelloLoginViewModel(vaultFolder)
                            : throw new NotSupportedException($"The authentication method '{item}' is not supported by the platform."),

                    // Key File
                    Core.Constants.Vault.Authentication.AUTH_KEYFILE => new KeyFileLoginViewModel(vaultFolder),

                    _ => throw new NotSupportedException($"The authentication method '{item}' is not supported by the platform.")
                };
            }
        }

        /// <inheritdoc/>
        public override async IAsyncEnumerable<AuthenticationViewModel> GetCreationAsync(IFolder vaultFolder, string vaultId,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Password
            yield return new PasswordCreationViewModel();

            // Windows Hello
            if (await KeyCredentialManager.IsSupportedAsync().AsTask(cancellationToken))
                yield return new WindowsHelloCreationViewModel(vaultFolder, vaultId);

            // Key File
            yield return new KeyFileCreationViewModel(vaultId);
        }
    }
}
