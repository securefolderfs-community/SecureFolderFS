using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard.NewVault
{
    [Inject<IPrinterService>, Inject<IThreadingService>]
    public sealed partial class RecoveryKeyWizardViewModel : BaseWizardPageViewModel
    {
        private readonly IFolder _vaultFolder;
        private readonly IDisposable _superSecret;

        [ObservableProperty] private string? _MasterKey;

        public RecoveryKeyWizardViewModel(IFolder vaultFolder, IDisposable superSecret, VaultWizardDialogViewModel dialogViewModel)
            : base(dialogViewModel)
        {
            ServiceProvider = Ioc.Default;
            _vaultFolder = vaultFolder;
            _superSecret = superSecret;
        }

        /// <inheritdoc/>
        public override async Task PrimaryButtonClickAsync(IEventDispatch? eventDispatch, CancellationToken cancellationToken)
        {
            eventDispatch?.PreventForwarding();

            // Add the newly created vault
            var vaultModel = new VaultModel(_vaultFolder);
            DialogViewModel.VaultCollectionModel.Add(vaultModel);

            // Try to save the new vault
            await DialogViewModel.VaultCollectionModel.TrySaveAsync(cancellationToken);

            // Navigate
            await NavigationService.TryNavigateAsync(() => new SummaryWizardViewModel(_vaultFolder.Name, DialogViewModel));
        }

        /// <inheritdoc/>
        public override void OnDisappearing()
        {
            _superSecret.Dispose();
            MasterKey = null;
        }

        [RelayCommand]
        private async Task PrintAsync(CancellationToken cancellationToken)
        {
            await ThreadingService.ChangeThreadAsync();

            if (await PrinterService.IsSupportedAsync())
                await PrinterService.PrintMasterKeyAsync(_superSecret, _vaultFolder.Name);
        }

        [RelayCommand]
        private void RevealMasterKey()
        {
            MasterKey ??= _superSecret.ToString();
        }
    }
}
