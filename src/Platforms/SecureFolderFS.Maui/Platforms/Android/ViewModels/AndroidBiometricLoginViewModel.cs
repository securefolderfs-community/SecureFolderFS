using OwlCore.Storage;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.EventArguments;
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
            var auth = await vaultReader.ReadAuthenticationAsync($"{Id}{Core.Constants.Vault.Names.CONFIGURATION_EXTENSION}", cancellationToken);

            if (auth?.Challenge is null)
            {
                Report(Result.Failure(new ArgumentNullException(nameof(auth))));
                return;
            }

            try
            {
                // Ask for credentials
                var key = await SignAsync(VaultId, auth.Challenge, cancellationToken);

                // Report that credentials were provided and new provision needs to be applied
                CredentialsProvided?.Invoke(this, new CredentialsProvidedEventArgs(key));

                // TODO: Provision is currently disabled since it opens the Android Biometrics dialog for the second time
                //StateChanged?.Invoke(this, new CredentialsProvisionChangedEventArgs(newChallenge.CreateCopy(), newSignedChallenge.CreateCopy()));
            }
            catch (Exception ex)
            {
                Report(Result.Failure(ex));
            }
        }
    }
}
