using OwlCore.Storage;
using SecureFolderFS.Core;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Maui.Platforms.Android.ViewModels
{
    internal sealed class AndroidBiometricLoginViewModel(IFolder vaultFolder, string vaultId) : AndroidBiometricViewModel(vaultFolder, vaultId)
    {
        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        /// <inheritdoc/>
        public override event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;

        /// <inheritdoc/>
        protected override async Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
            var vaultReader = new VaultReader(VaultFolder, StreamSerializer.Instance);
            var auth = await vaultReader.ReadAuthenticationAsync<VaultChallengeDataModel>($"{Id}{Constants.Vault.Names.CONFIGURATION_EXTENSION}", cancellationToken);

            if (auth?.Challenge is null)
            {
                Report(Result.Failure(new ArgumentNullException(nameof(auth))));
                return;
            }

            try
            {
                // Ask for credentials
                var keyResult = await AcquireAsync(VaultId, auth.Challenge, cancellationToken);
                if (!keyResult.TryGetValue(out var key))
                {
                    Report(keyResult);
                    return;
                }
                
                // Report that credentials were provided and new provision needs to be applied
                var tcs = new TaskCompletionSource();
                CredentialsProvided?.Invoke(this, new CredentialsProvidedEventArgs(key, tcs));
                await tcs.Task;
            }
            catch (Exception ex)
            {
                Report(Result.Failure(ex));
            }
        }
    }
}
