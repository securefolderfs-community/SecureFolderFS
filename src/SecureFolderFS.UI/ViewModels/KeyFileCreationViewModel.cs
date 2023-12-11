using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.UI.ViewModels
{
    public sealed partial class KeyFileCreationViewModel : KeyFileViewModel
    {
        private readonly string _vaultId;

        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        public KeyFileCreationViewModel(string vaultId, string id, IFolder vaultFolder)
            : base(id, vaultFolder)
        {
            _vaultId = vaultId;
        }

        [RelayCommand]
        private async Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
            var key = await CreateAsync(_vaultId, null, cancellationToken);
            StateChanged?.Invoke(this, new AuthenticationChangedEventArgs(key));
        }
    }
}
