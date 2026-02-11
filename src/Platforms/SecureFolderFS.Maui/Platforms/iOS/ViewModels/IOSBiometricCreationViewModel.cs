using System.Security.Cryptography;
using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Cryptography.Extensions;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Maui.Platforms.iOS.ViewModels
{
    internal sealed class IOSBiometricCreationViewModel : IOSBiometricViewModel
    {
        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;
        
        /// <inheritdoc/>
        public override event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;
        
        public IOSBiometricCreationViewModel(IFolder vaultFolder, string vaultId, string title)
            : base(vaultFolder, vaultId, title)
        {
        }

        /// <inheritdoc/>
        protected override async Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
            using var keyMaterial = new ManagedKey(Constants.KeyTraits.ECIES_SHA256_AESGCM_STDX963_KEY_LENGTH);
            RandomNumberGenerator.Fill(keyMaterial.Key);

            try
            {
                IsAuthenticated = false;
                var keyResult = await EnrollAsync(VaultId, keyMaterial.Key, cancellationToken);
                IsAuthenticated = true;
                
                if (!keyResult.TryGetValue(out var ciphertextKey))
                {
                    Report(keyResult);
                    return;
                }
                
                var tcs = new TaskCompletionSource();
                CredentialsProvided?.Invoke(this, new CredentialsProvidedEventArgs(keyMaterial.CreateCopy(), tcs));
                await tcs.Task;

                // Write the ciphertext key
                var vaultWriter = new VaultWriter(VaultFolder, StreamSerializer.Instance);
                await vaultWriter.WriteAuthenticationAsync<VaultProtectedKeyDataModel>($"{Id}{Core.Constants.Vault.Names.CONFIGURATION_EXTENSION}", new()
                {
                    Capability = "supportsKeyProtection",
                    CiphertextKey = ciphertextKey.Key
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                Report(Result.Failure(ex));
            }
        }
    }
}
