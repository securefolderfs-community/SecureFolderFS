using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard.ExistingVault
{
    public sealed partial class ExistingLocationWizardViewModel : BaseWizardPageViewModel
    {
        private readonly IAsyncValidator<IFolder> _vaultValidator;
        private IFolder? _vaultFolder;

        private IFileExplorerService FileExplorerService { get; } = Ioc.Default.GetRequiredService<IFileExplorerService>();

        [ObservableProperty] private string? _SelectedLocationText = "No folder selected";

        public ExistingLocationWizardViewModel(VaultWizardDialogViewModel dialogViewModel)
            : base(dialogViewModel)
        {
            _vaultValidator = Ioc.Default.GetRequiredService<IVaultService>().GetVaultValidator();
        }

        public override async Task PrimaryButtonClickAsync(IEventDispatch? eventDispatch, CancellationToken cancellationToken)
        {
            eventDispatch?.NoForwarding();
            if (_vaultFolder is null)
                return;

            var vaultModel = new VaultModel(_vaultFolder);
            DialogViewModel.VaultCollectionModel.AddVault(vaultModel);
            await DialogViewModel.VaultCollectionModel.SaveAsync(cancellationToken);

            WeakReferenceMessenger.Default.Send(new AddVaultMessage(vaultModel));
            await NavigationService.TryNavigateAsync(() => new SummaryWizardViewModel(vaultModel.VaultName, DialogViewModel));
        }

        [RelayCommand]
        private async Task BrowseLocationAsync(CancellationToken cancellationToken)
        {
            _vaultFolder = await FileExplorerService.PickSingleFolderAsync(cancellationToken);
            if (_vaultFolder is null)
                return;

            var validationResult = await _vaultValidator.ValidateAsync(_vaultFolder, cancellationToken);
            if (!validationResult.Successful)
                return;

            SelectedLocationText = _vaultFolder.Name;
            DialogViewModel.PrimaryButtonEnabled = true;
        }
    }
}
