using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.Authenticators;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.WinUI.Authenticators;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Windows.Security.Credentials;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultManagerService"/>
    public sealed class WindowsVaultManagerService : BaseVaultManagerService
    {
        /// <inheritdoc/>
        public override async IAsyncEnumerable<AuthenticationModel> GetAvailableAuthenticationsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Password
            yield return new(
                Core.Constants.Vault.AuthenticationMethods.AUTH_PASSWORD,
                "Password",
                AuthenticationType.Password,
                null);

            // Windows Hello
            if (await KeyCredentialManager.IsSupportedAsync().AsTask(cancellationToken))
                yield return new(
                    Core.Constants.Vault.AuthenticationMethods.AUTH_WINDOWS_HELLO,
                    "Windows Hello",
                    AuthenticationType.Other,
                    new WindowsHelloAuthenticator());
            
            // Key File
            yield return new(
                Core.Constants.Vault.AuthenticationMethods.AUTH_KEYFILE,
                "Key File",
                AuthenticationType.Other,
                new KeyFileAuthenticator());

            // Hardware Key
            yield return new(
                Core.Constants.Vault.AuthenticationMethods.AUTH_HARDWARE_KEY,
                "Hardware File",
                AuthenticationType.Other,
                null);
        }
    }
}
