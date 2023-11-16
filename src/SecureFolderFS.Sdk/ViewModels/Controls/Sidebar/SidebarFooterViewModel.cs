using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Shared.Extensions;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Sidebar
{
    [Inject<IIapService>, Inject<IDialogService>, Inject<ISettingsService>]
    public sealed partial class SidebarFooterViewModel : ObservableObject
    {
        private readonly IVaultCollectionModel _vaultCollectionModel;

        public SidebarFooterViewModel(IVaultCollectionModel vaultCollectionModel)
        {
            ServiceProvider = Ioc.Default;
            _vaultCollectionModel = vaultCollectionModel;
        }

        [RelayCommand]
        private async Task AddNewVaultAsync(CancellationToken cancellationToken)
        {
            var isPremiumOwned = await IapService.IsOwnedAsync(IapProductType.SecureFolderFSPlus, cancellationToken);
            if (_vaultCollectionModel.Count >= 2 && !isPremiumOwned)
            {
                _ = PaymentDialogViewModel.Instance.InitAsync(cancellationToken);
                await DialogService.ShowDialogAsync(PaymentDialogViewModel.Instance);
            }
            else
                await DialogService.ShowDialogAsync(new VaultWizardDialogViewModel(_vaultCollectionModel));
        }

        [RelayCommand]
        private async Task OpenSettingsAsync(CancellationToken cancellationToken)
        {
            await DialogService.ShowDialogAsync(SettingsDialogViewModel.Instance);
            await SettingsService.TrySaveAsync(cancellationToken);
        }
    }
}
