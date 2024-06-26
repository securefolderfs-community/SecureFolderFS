using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.Uno.ViewModels
{
    /// <inheritdoc cref="WindowsHelloViewModel"/>
    [Bindable(true)]
    public sealed class WindowsHelloCreationViewModel(string vaultId, string id, IFolder vaultFolder)
        : WindowsHelloViewModel(id)
    {
        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        /// <inheritdoc/>
        public override event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;

        /// <inheritdoc/>
        protected override async Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
            var vaultWriter = new VaultWriter(vaultFolder, StreamSerializer.Instance);
            using var challenge = GenerateChallenge(vaultId);

            // Write authentication data to the vault
            await vaultWriter.WriteAuthenticationAsync(new()
            {
                Capability = "supportsChallenge", // TODO: Put somewhere in Constants
                Challenge = challenge.Key
            }, cancellationToken);

            try
            {
                var key = await this.TryCreateAsync(vaultId, challenge.Key, cancellationToken);
                if (key is { Successful: true, Value: not null })
                    CredentialsProvided?.Invoke(this, new(key.Value));
            }
            catch (InvalidOperationException iopex)
            {
                // Thrown when authentication is canceled
                SetError(Result.Failure(iopex));
            }
        }
    }
}
