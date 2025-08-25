using OwlCore.Storage;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Maui.Platforms.iOS.ViewModels
{
    internal sealed class IOSSecureEnclaveLoginViewModel : IOSSecureEnclaveViewModel
    {
        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;
        
        /// <inheritdoc/>
        public override event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;

        public IOSSecureEnclaveLoginViewModel(IFolder vaultFolder, string vaultId)
            : base(vaultFolder, vaultId)
        {
        }
        
        /// <inheritdoc/>
        protected override async Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
            var vaultReader = new SecureFolderFS.Core.VaultAccess.VaultReader(VaultFolder, StreamSerializer.Instance);
            var auth = await vaultReader.ReadAuthenticationAsync($"{Id}{Core.Constants.Vault.Names.CONFIGURATION_EXTENSION}", cancellationToken);

            if (auth?.Challenge is null)
            {
                Report(Result.Failure(new ArgumentNullException(nameof(auth))));
                return;
            }
            
            try
            {
                var key = await SignAsync(VaultId, auth.Challenge, cancellationToken);
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
