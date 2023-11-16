using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard.NewVault;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard
{
    public sealed partial class MainWizardPageViewModel : BaseWizardPageViewModel
    {
        private readonly WizardChoiceViewModel _existingViewModel;
        private readonly WizardChoiceViewModel _createNewViewModel;

        [ObservableProperty] private WizardChoiceViewModel? _CurrentViewModel;

        public MainWizardPageViewModel(VaultWizardDialogViewModel dialogViewModel)
            : base(dialogViewModel)
        {
            _existingViewModel = new(NewVaultCreationType.AddExisting, DialogViewModel);
            _createNewViewModel = new(NewVaultCreationType.CreateNew, DialogViewModel);
            DialogViewModel.PrimaryButtonEnabled = false;
        }

        public override async Task PrimaryButtonClickAsync(IEventDispatch? eventDispatch, CancellationToken cancellationToken)
        {
            eventDispatch?.NoForwarding();
            if (CurrentViewModel?.VaultFolder is null)
                return;

            if (CurrentViewModel == _existingViewModel) // Add existing
            {
                // Add the newly created vault
                var vaultModel = new VaultModel(CurrentViewModel.VaultFolder);
                DialogViewModel.VaultCollectionModel.Add(vaultModel);

                // Try to save the new vault
                await DialogViewModel.VaultCollectionModel.TrySaveAsync(cancellationToken);

                // Navigate
                await NavigationService.TryNavigateAsync(() => new SummaryWizardViewModel(vaultModel.VaultName, DialogViewModel));
            }
            else // Create new
            {
                // Navigate to next page
                await NavigationService.TryNavigateAsync(() => new AuthCreationWizardViewModel((IModifiableFolder)CurrentViewModel.VaultFolder, DialogViewModel));
            }
        }

        public async Task UpdateSelectionAsync(NewVaultCreationType creationType, CancellationToken cancellationToken)
        {
            CurrentViewModel = creationType == NewVaultCreationType.CreateNew ? _createNewViewModel : _existingViewModel;
            DialogViewModel.PrimaryButtonEnabled = await CurrentViewModel.UpdateStatusAsync(cancellationToken);
        }
    }
}
