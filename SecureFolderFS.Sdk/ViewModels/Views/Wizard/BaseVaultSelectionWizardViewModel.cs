using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard
{
    [Inject<IFileExplorerService>(Visibility = "protected")]
    public abstract partial class BaseVaultSelectionWizardViewModel : BaseWizardPageViewModel
    {
        protected IFolder? vaultFolder;

        [ObservableProperty] private InfoBarViewModel _SelectionInfoBar;

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
            await UpdateStatusAsync(default);
        }

        protected virtual Task<bool> UpdateStatusAsync(CancellationToken cancellationToken)
        {
            if (vaultFolder is null)
            {
                SelectionInfoBar.Severity = InfoBarSeverityType.Information;
                SelectionInfoBar.Message = "No vault selected";
                return Task.FromResult(false);
            }

            SelectionInfoBar.Severity = InfoBarSeverityType.Success;
            SelectionInfoBar.Message = vaultFolder.Name;
            return Task.FromResult(true);
        }

        [RelayCommand]
        protected virtual async Task OpenFolderAsync(CancellationToken cancellationToken)
        {
            vaultFolder = await FileExplorerService.PickFolderAsync(cancellationToken);
            DialogViewModel.PrimaryButtonEnabled = await UpdateStatusAsync(cancellationToken);
        }
    }
}
