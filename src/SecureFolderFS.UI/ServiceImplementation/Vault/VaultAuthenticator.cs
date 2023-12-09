using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services.Vault;
using SecureFolderFS.Sdk.Storage;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.UI.ServiceImplementation.Vault
{
    public sealed class VaultAuthenticator : IVaultAuthenticator
    {
        /// <inheritdoc/>
        public async IAsyncEnumerable<AuthenticationModel> GetAuthenticationAsync(IFolder vaultFolder, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;

            yield return new(Core.Constants.Vault.AuthenticationMethods.AUTH_WINDOWS_HELLO, "Password", AuthenticationType.Password, null);
            //yield return new("Windows Hello", AuthenticationType.Other, );
            //yield return new("Key File", AuthenticationType.Other, new KeyFileAuthenticator());
        }

        public async IAsyncEnumerable<AuthenticationModel> GetAvailableAuthenticationsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return new(Core.Constants.Vault.AuthenticationMethods.AUTH_PASSWORD, "Password", AuthenticationType.Password, null);
            yield return new(Core.Constants.Vault.AuthenticationMethods.AUTH_KEYFILE, "Key File", AuthenticationType.Other, null);
            yield return new(Core.Constants.Vault.AuthenticationMethods.AUTH_HARDWARE_KEY, "Hardware File", AuthenticationType.Other, null);
            yield return new(Core.Constants.Vault.AuthenticationMethods.AUTH_APPLE_FACEID, "Face ID", AuthenticationType.Other, null);
            yield return new(Core.Constants.Vault.AuthenticationMethods.AUTH_WINDOWS_HELLO, "Windows Hello", AuthenticationType.Other, null);
            yield return new(Core.Constants.Vault.AuthenticationMethods.AUTH_ANDROID_BIOMETRIC, "Biometrics", AuthenticationType.Other, null);

            await Task.CompletedTask;
        }
    }
}
