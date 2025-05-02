using OwlCore.Storage;
using SecureFolderFS.Core;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.Helpers;

namespace SecureFolderFS.Maui.Platforms.Android.ViewModels
{
    internal sealed class AndroidBiometricCreationViewModel(IFolder vaultFolder, string vaultId) : AndroidBiometricViewModel(vaultFolder, vaultId)
    {
        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;
        
        /// <inheritdoc/>
        public override event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;
        
        /// <inheritdoc/>
        protected override async Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
            var vaultWriter = new VaultWriter(VaultFolder, StreamSerializer.Instance);
            using var challenge = VaultHelpers.GenerateChallenge(VaultId);

            // Write authentication data to the vault
            await vaultWriter.WriteAuthenticationAsync($"{Id}{Constants.Vault.Names.CONFIGURATION_EXTENSION}", new()
            {
                Capability = "supportsChallenge", // TODO: Put somewhere in Constants
                Challenge = challenge.Key
            }, cancellationToken);

            try
            {
                var key = await CreateAsync(VaultId, challenge.Key, cancellationToken);
                CredentialsProvided?.Invoke(this, new(key));
            }
            catch (Exception ex)
            {
                Report(Result.Failure(ex));
            }
        }
    }
}
