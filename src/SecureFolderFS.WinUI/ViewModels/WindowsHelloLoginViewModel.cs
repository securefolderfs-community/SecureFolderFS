using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.WinUI.ViewModels
{
    public sealed partial class WindowsHelloLoginViewModel : WindowsHelloViewModel
    {
        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        public WindowsHelloLoginViewModel(string id, IFolder vaultFolder)
            : base(id, vaultFolder)
        {
        }

        [RelayCommand]
        private async Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
            byte[] challenge = Array.Empty<byte>(); // TODO: Read data to sign and generate new to save it to the auth file

            var key = await SignAsync(Id, challenge, cancellationToken);
            StateChanged?.Invoke(this, new AuthenticationChangedEventArgs(key));
        }
    }
}
