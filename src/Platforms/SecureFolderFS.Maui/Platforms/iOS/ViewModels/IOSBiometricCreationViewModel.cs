using OwlCore.Storage;
using SecureFolderFS.Core;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.Helpers;

namespace SecureFolderFS.Maui.Platforms.iOS.ViewModels
{
    internal sealed class IOSBiometricCreationViewModel : IOSBiometricViewModel
    {
        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;
        
        /// <inheritdoc/>
        public override event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;
        
        public IOSBiometricCreationViewModel(IFolder vaultFolder, string vaultId)
            : base(vaultFolder, vaultId)
        {
        }

        /// <inheritdoc/>
        protected override async Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
            var vaultWriter = new VaultWriter(VaultFolder, StreamSerializer.Instance);
            using var keyMaterial = VaultHelpers.GenerateKeyMaterial();

            try
            {
                var ciphertextKey = await EnrollAsync(VaultId, keyMaterial.Key, cancellationToken);
                var tcs = new TaskCompletionSource();
                CredentialsProvided?.Invoke(this, new CredentialsProvidedEventArgs(keyMaterial, tcs));
                await tcs.Task;

                // Write the ciphertext key
                await vaultWriter.WriteAuthenticationAsync<VaultProtectedKeyDataModel>($"{Id}{Constants.Vault.Names.CONFIGURATION_EXTENSION}", new()
                {
                    Capability = "supportsKeyProtection",
                    CiphertextKey = ciphertextKey.ToArray()
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                Report(Result.Failure(ex));
            }
        }
    }
}
