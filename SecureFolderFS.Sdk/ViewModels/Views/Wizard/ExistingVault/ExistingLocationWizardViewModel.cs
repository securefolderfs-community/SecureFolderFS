using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard.ExistingVault
{
    [Inject<IFileExplorerService>, Inject<IVaultService>]
    public sealed partial class ExistingLocationWizardViewModel : BaseWizardPageViewModel
    {
        private IFolder? _vaultFolder;
        private readonly IAsyncValidator<IFolder> _vaultValidator;

        [ObservableProperty] private string? _SelectedLocationText = "NoFolderSelected".ToLocalized();

        public ExistingLocationWizardViewModel(VaultWizardDialogViewModel dialogViewModel)
            : base(dialogViewModel)
        {
            ServiceProvider = Ioc.Default;
            _vaultValidator = VaultService.GetVaultValidator();
        }

        public override async Task PrimaryButtonClickAsync(IEventDispatch? eventDispatch, CancellationToken cancellationToken)
        {
            eventDispatch?.NoForwarding();
            if (_vaultFolder is null)
                return;

            var vaultModel = new VaultModel(_vaultFolder);
            DialogViewModel.VaultCollectionModel.Add(vaultModel);
            await DialogViewModel.VaultCollectionModel.TrySaveAsync(cancellationToken);

            await NavigationService.TryNavigateAsync(() => new SummaryWizardViewModel(vaultModel.VaultName, DialogViewModel));
        }

        [RelayCommand]
        private async Task BrowseLocationAsync(CancellationToken cancellationToken)
        {
            _vaultFolder = await FileExplorerService.PickFolderAsync(cancellationToken);
            if (_vaultFolder is null)
                return;

            var validationResult = await _vaultValidator.TryValidateAsync(_vaultFolder, cancellationToken);
            if (!validationResult.Successful)
                return;

            SelectedLocationText = _vaultFolder.Name;
            DialogViewModel.PrimaryButtonEnabled = true;
        }
    }
}
