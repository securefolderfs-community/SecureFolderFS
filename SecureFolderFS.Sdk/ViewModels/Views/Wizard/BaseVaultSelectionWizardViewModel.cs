using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard
{
    [Inject<IFileExplorerService>(Visibility = "protected")]
    public abstract partial class BaseVaultSelectionWizardViewModel : BaseWizardPageViewModel
    {
        protected IFolder? vaultFolder;

        [ObservableProperty] private InfoBarViewModel _SelectionInfoBar;
        [ObservableProperty] private string _SelectedLocation;

        public BaseVaultSelectionWizardViewModel(VaultWizardDialogViewModel dialogViewModel)
            : base(dialogViewModel)
        {
            ServiceProvider = Ioc.Default;
            _SelectionInfoBar = new();
        }

        /// <inheritdoc/>
        public override async void OnNavigatingTo(NavigationType navigationType)
        {
            _ = navigationType;
            await UpdateStatusInternalAsync(default);
        }

        protected Task<bool> UpdateStatusInternalAsync(CancellationToken cancellationToken)
        {
            if (vaultFolder is null)
            {
                SelectionInfoBar.Severity = InfoBarSeverityType.Information;
                SelectionInfoBar.Message = "Select a vault folder to continue";
                SelectedLocation = "No vault selected";
                return Task.FromResult(false);
            }

            SelectedLocation = $"..{Path.DirectorySeparatorChar}{Path.GetFileName(Path.GetDirectoryName(vaultFolder.Id))}{Path.DirectorySeparatorChar}{Path.GetFileName(vaultFolder.Id)}";
            return UpdateStatusAsync(cancellationToken);
        }

        [RelayCommand]
        protected virtual async Task OpenFolderAsync(CancellationToken cancellationToken)
        {
            vaultFolder = await FileExplorerService.PickFolderAsync(cancellationToken);
            DialogViewModel.PrimaryButtonEnabled = await UpdateStatusInternalAsync(cancellationToken);
        }

        protected abstract Task<bool> UpdateStatusAsync(CancellationToken cancellationToken);
    }
}
