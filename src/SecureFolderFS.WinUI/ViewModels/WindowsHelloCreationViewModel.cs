using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.WinUI.ViewModels
{
    public sealed partial class WindowsHelloCreationViewModel : WindowsHelloViewModel
    {
        private readonly string _vaultId;

        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        public WindowsHelloCreationViewModel(string vaultId, string id, IFolder vaultFolder)
            : base(id, vaultFolder)
        {
            _vaultId = vaultId;
        }

        [RelayCommand]
        private async Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
            byte[] challenge = Array.Empty<byte>(); // TODO: Create data to sign and save it to the auth file

            var key = await SignAsync(_vaultId, challenge, cancellationToken);
            StateChanged?.Invoke(this, new AuthenticationChangedEventArgs(key));
        }
    }
}
