using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Helpers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Uno.ViewModels
{
    public sealed partial class WindowsHelloLoginViewModel : WindowsHelloViewModel
    {
        private const int KEY_LENGTH = 128;

        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        public WindowsHelloLoginViewModel(string id, IFolder vaultFolder)
            : base(id, vaultFolder)
        {
        }

        [RelayCommand]
        private async Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
            var vaultReader = new VaultReader(VaultFolder, StreamSerializer.Instance);
            var vaultWriter = new VaultWriter(VaultFolder, StreamSerializer.Instance);

            var config = await vaultReader.ReadConfigurationAsync(cancellationToken);
            var auth = await vaultReader.ReadAuthenticationAsync(cancellationToken);
            if (auth?.Challenge is null)
            {
                SetError(CommonResult.Failure(new ArgumentNullException(nameof(auth))));
                return;
            }

            var key = await SignAsync(config.Id, auth.Challenge, cancellationToken);
            StateChanged?.Invoke(this, new AuthenticationChangedEventArgs(key));

            // TODO: Read data to sign and generate new to save it to the auth file
            _ = config;
            _ = vaultWriter;
        }
    }
}
