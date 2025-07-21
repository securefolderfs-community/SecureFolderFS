using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.Helpers;

namespace SecureFolderFS.Uno.ViewModels
{
    /// <inheritdoc cref="WindowsHelloViewModel"/>
    [Bindable(true)]
    public sealed class WindowsHelloCreationViewModel(IFolder vaultFolder, string vaultId) : WindowsHelloViewModel(vaultFolder, vaultId)
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
                var tcs = new TaskCompletionSource();
                CredentialsProvided?.Invoke(this, new(key, tcs));

                await tcs.Task;
            }
            catch (Exception ex)
            {
                // Thrown when authentication is canceled
                Report(Result.Failure(ex));
            }
        }
    }
}
