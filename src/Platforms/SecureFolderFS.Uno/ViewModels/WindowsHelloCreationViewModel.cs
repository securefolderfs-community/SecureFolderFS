using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.Uno.ViewModels
{
    public sealed partial class WindowsHelloCreationViewModel : WindowsHelloViewModel
    {
        private readonly string _vaultId;

        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        /// <inheritdoc/>
        public override event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;

        public WindowsHelloCreationViewModel(string vaultId, string id, IFolder vaultFolder)
            : base(id, vaultFolder)
        {
            _vaultId = vaultId;
        }

        [RelayCommand]
        private async Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
            var vaultWriter = new VaultWriter(VaultFolder, StreamSerializer.Instance);
            using var challenge = GenerateChallenge(_vaultId);

            // Write authentication data to the vault
            await vaultWriter.WriteAuthenticationAsync(new()
            {
                Capability = "supportsChallenge", // TODO: Put somewhere in Constants
                Challenge = challenge.Key
            }, cancellationToken);

            try
            {
                var key = await this.TryCreateAsync(_vaultId, challenge.Key, cancellationToken);
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
