using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Uno.ViewModels
{
    /// <inheritdoc cref="YubiKeyViewModel"/>
    [Bindable(true)]
    public sealed class YubiKeyLoginViewModel(IFolder vaultFolder, string vaultId) : YubiKeyViewModel(vaultFolder, vaultId)
    {
        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        /// <inheritdoc/>
        public override event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;

        /// <inheritdoc/>
        protected override async Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
            var vaultReader = new VaultReader(VaultFolder, StreamSerializer.Instance);
            var auth = await vaultReader.ReadAuthenticationAsync<VaultChallengeDataModel>($"{Id}{Core.Constants.Vault.Names.CONFIGURATION_EXTENSION}", cancellationToken);

            if (auth?.Challenge is null)
            {
                Report(Result.Failure(new ArgumentNullException(nameof(auth))));
                return;
            }

            // Set the slot preference from the saved configuration
            UseLongPress = auth.Capability.Contains("useLongPress");

            try
            {
                // Generate key signature based on the YubiKey challenge-response
                var key = await AcquireAsync(VaultId, auth.Challenge, cancellationToken);
                var tcs = new TaskCompletionSource();

                // Report that credentials were provided
                CredentialsProvided?.Invoke(this, new CredentialsProvidedEventArgs(key, tcs));
                await tcs.Task;
            }
            catch (Exception ex)
            {
                // Thrown when authentication fails or YubiKey is not present
                Report(Result.Failure(ex));
            }
        }
    }
}

