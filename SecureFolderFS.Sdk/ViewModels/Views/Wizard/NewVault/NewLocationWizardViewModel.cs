using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard.NewVault
{
    [Inject<IVaultService>, Inject<IFileExplorerService>]
    public sealed partial class NewLocationWizardViewModel : BaseWizardPageViewModel
    {
        private readonly IVaultCreationModel _vaultCreationModel;
        private IModifiableFolder? _vaultFolder;

        [ObservableProperty] private string? _SelectedLocationText = "NoFolderSelected".ToLocalized();

        public NewLocationWizardViewModel(VaultWizardDialogViewModel dialogViewModel)
            : base(dialogViewModel)
        {
            ServiceProvider = Ioc.Default;
            _vaultCreationModel = new VaultCreationModel();
        }

        public override async Task PrimaryButtonClickAsync(IEventDispatch? eventDispatch, CancellationToken cancellationToken)
        {
            eventDispatch?.NoForwarding();

            if (_vaultFolder is null)
                return;

            var navigationResult = await NavigationService.TryNavigateAsync<PasswordWizardViewModel>();
            if (navigationResult)
                return; // Next view model was already present, skip initialization

            var keystoreFile = await _vaultFolder.TryCreateFileAsync(VaultService.KeystoreFileName, false, cancellationToken);
            if (keystoreFile is null)
                return; // TODO: Report issue

            var setFolderResult = await _vaultCreationModel.SetFolderAsync(_vaultFolder, cancellationToken);
            if (!setFolderResult.Successful)
                return; // TODO: Report issue

            var setKeystoreResult = await _vaultCreationModel.SetKeystoreAsync(new FileKeystoreModel(keystoreFile, StreamSerializer.Instance), cancellationToken);
            if (!setKeystoreResult.Successful)
                return; // TODO: Report issue

            await NavigationService.NavigateAsync(new PasswordWizardViewModel(_vaultCreationModel, DialogViewModel));
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
