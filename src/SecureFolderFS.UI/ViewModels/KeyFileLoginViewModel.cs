using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.UI.ViewModels
{
    public sealed partial class KeyFileLoginViewModel : KeyFileViewModel
    {
        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        public KeyFileLoginViewModel(string id, IFolder vaultFolder)
            : base(id, vaultFolder)
        {
        }

        [RelayCommand]
        private async Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
            var vaultReader = new VaultReader(VaultFolder, StreamSerializer.Instance);
            var config = await vaultReader.ReadConfigurationAsync(cancellationToken);
            var key = await SignAsync(config.Id, null, cancellationToken);

            StateChanged?.Invoke(this, new AuthenticationChangedEventArgs(key));
        }
    }
}
