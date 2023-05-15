using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Sidebar
{
    public sealed partial class SidebarFooterViewModel : ObservableObject
    {
        private readonly IVaultCollectionModel _vaultCollectionModel;

        private IIapService IapService { get; } = Ioc.Default.GetRequiredService<IIapService>();

        private IDialogService DialogService { get; } = Ioc.Default.GetRequiredService<IDialogService>();

        private ISettingsService SettingsService { get; } = Ioc.Default.GetRequiredService<ISettingsService>();

        public SidebarFooterViewModel(IVaultCollectionModel vaultCollectionModel)
        {
            _vaultCollectionModel = vaultCollectionModel;
        }

        [RelayCommand]
        private async Task AddNewVaultAsync(CancellationToken cancellationToken)
        {
            var isPremiumOwned = await IapService.IsOwnedAsync(IapProductType.SecureFolderFSPlus, cancellationToken);
            if (_vaultCollectionModel.GetVaults().Count() >= 2 && !isPremiumOwned)
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
            await SettingsService.SaveAsync(cancellationToken);
        }
    }
}
