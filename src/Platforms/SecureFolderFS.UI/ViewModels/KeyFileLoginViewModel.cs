using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.UI.ViewModels
{
    public sealed partial class KeyFileLoginViewModel : KeyFileViewModel
    {
        /// <inheritdoc/>
        public override event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;

        public KeyFileLoginViewModel(string id, IFolder vaultFolder)
            : base(id, vaultFolder)
        {
        }

        [RelayCommand]
        private async Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
            var vaultReader = new VaultReader(VaultFolder, StreamSerializer.Instance);
            var config = await vaultReader.ReadConfigurationAsync(cancellationToken);
            var key = await this.TrySignAsync(config.Id, null, cancellationToken);
            if (!key.Successful || key.Value is null)
                return;

            CredentialsProvided?.Invoke(this, new(key.Value));
        }
    }
}
