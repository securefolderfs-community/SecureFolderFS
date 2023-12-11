using CommunityToolkit.Mvvm.Input;
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
            var vaultId = "9761f3c1-bea0-4216-be82-81e2654b7a9b"; // TODO: Temporary ID. Read the ID from the file
            var key = await SignAsync(vaultId, null, cancellationToken);
            StateChanged?.Invoke(this, new AuthenticationChangedEventArgs(key));
        }
    }
}
