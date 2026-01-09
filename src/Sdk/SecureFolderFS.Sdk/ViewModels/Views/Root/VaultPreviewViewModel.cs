using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Shared.ComponentModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Root
{
    [Bindable(true)]
    public sealed partial class VaultPreviewViewModel : ObservableObject, IAsyncInitialize, IRecipient<VaultLockedMessage>
    {
        [ObservableProperty] private LoginViewModel? _LoginViewModel;
        [ObservableProperty] private VaultViewModel _VaultViewModel;
        [ObservableProperty] private bool _IsReadOnly;

        public VaultPreviewViewModel(LoginViewModel loginViewModel, VaultViewModel vaultViewModel)
            : this(vaultViewModel)
        {
            LoginViewModel = loginViewModel;
        }

        public VaultPreviewViewModel(VaultViewModel vaultViewModel)
        {
            VaultViewModel = vaultViewModel;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            if (LoginViewModel is not null)
            {
                await LoginViewModel.InitAsync();
                // TODO: Hook up events to LoginViewModel
            }
        }

        /// <inheritdoc/>
        public void Receive(VaultLockedMessage message)
        {
            if (message.VaultModel != VaultViewModel.VaultModel)
                return;

            if (VaultViewModel.VaultModel.VaultFolder is null)
                return;

            LoginViewModel?.Dispose();
            LoginViewModel = new(VaultViewModel.VaultModel.VaultFolder, LoginViewType.Constrained);
            _ = InitAsync();
        }
    }
}
