using OwlCore.Storage;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.Helpers;

namespace SecureFolderFS.Maui.Platforms.iOS.ViewModels
{
    internal sealed class IOSSecureEnclaveCreationViewModel : IOSSecureEnclaveViewModel
    {
        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;
        
        /// <inheritdoc/>
        public override event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;
        
        public IOSSecureEnclaveCreationViewModel(IFolder vaultFolder, string vaultId)
            : base(vaultFolder, vaultId)
        {
        }
        
        /// <inheritdoc/>
        protected override async Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
            var vaultWriter = new VaultWriter(VaultFolder, StreamSerializer.Instance);
            using var challenge = VaultHelpers.GenerateChallenge(VaultId);
            
            await vaultWriter.WriteAuthenticationAsync($"{Id}{Core.Constants.Vault.Names.CONFIGURATION_EXTENSION}", new()
            {
                Capability = "supportsChallenge",
                Challenge = challenge.Key
            }, cancellationToken);
            
            try
            {
                var key = await CreateAsync(VaultId, challenge.Key, cancellationToken);
                var tcs = new TaskCompletionSource();
                CredentialsProvided?.Invoke(this, new SecureFolderFS.Sdk.EventArguments.CredentialsProvidedEventArgs(key, tcs));
                await tcs.Task;
            }
            catch (Exception ex)
            {
                Report(Result.Failure(ex));
            }
        }
    }
}
