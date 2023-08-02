using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.Vault;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Shared.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard.NewVault
{
    [Inject<IVaultService>, Inject<IFileExplorerService>]
    public sealed partial class NewLocationWizardViewModel : BaseWizardPageViewModel
    {
        private readonly IVaultCreator _vaultCreator;
        private IModifiableFolder? _vaultFolder;

        [ObservableProperty] private string? _SelectedLocationText = "NoFolderSelected".ToLocalized();

        public NewLocationWizardViewModel(VaultWizardDialogViewModel dialogViewModel)
            : base(dialogViewModel)
        {
            ServiceProvider = Ioc.Default;
            _vaultCreator = VaultService.VaultCreator;
        }

        public override async Task PrimaryButtonClickAsync(IEventDispatch? eventDispatch, CancellationToken cancellationToken)
        {
            eventDispatch?.NoForwarding();

            if (_vaultFolder is null)
                return;

            _ = await NavigationService.TryNavigateAsync<PasswordWizardViewModel>(() => new(_vaultFolder, _vaultCreator, DialogViewModel));
        }

        [RelayCommand]
        private async Task BrowseLocationAsync(CancellationToken cancellationToken)
        {
            var folder = await FileExplorerService.PickFolderAsync(cancellationToken);
            if (folder is null)
                return;

            _vaultFolder = folder as IModifiableFolder;
            if (_vaultFolder is null)
                return;

            SelectedLocationText = _vaultFolder.Name;
            DialogViewModel.PrimaryButtonEnabled = true;
        }
    }
}
