using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.Vault;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard.NewVault
{
    [Inject<IVaultService>]
    public sealed partial class NewLocationWizardViewModel : BaseVaultSelectionWizardViewModel
    {
        private readonly IVaultCreator _vaultCreator;

        public NewLocationWizardViewModel(VaultWizardDialogViewModel dialogViewModel)
            : base(dialogViewModel)
        {
            ServiceProvider = Ioc.Default;
            _vaultCreator = VaultService.VaultCreator;
        }

        /// <inheritdoc/>
        public override async Task PrimaryButtonClickAsync(IEventDispatch? eventDispatch, CancellationToken cancellationToken)
        {
            eventDispatch?.NoForwarding();
            if (vaultFolder is null)
                return;

            _ = await NavigationService.TryNavigateAsync<PasswordWizardViewModel>(() => new((IModifiableFolder)vaultFolder, _vaultCreator, DialogViewModel));
        }

        /// <inheritdoc/>
        protected override async Task<bool> UpdateStatusAsync(CancellationToken cancellationToken)
        {
            if (vaultFolder is null)
                return false;

            var validationResult = await VaultService.VaultValidator.TryValidateAsync(vaultFolder, cancellationToken);
            if (validationResult.Successful || validationResult.Exception is NotSupportedException)
            {
                // Check if a valid (or unsupported) vault exists at a specified path
                SelectionInfoBar.Severity = InfoBarSeverityType.Warning;
                SelectionInfoBar.Message = "The selected vault will be overwritten";
                return true;
            }

            SelectionInfoBar.Severity = InfoBarSeverityType.Success;
            SelectionInfoBar.Message = "A new vault will be created in selected folder";
            return true;
        }

        protected override async Task OpenFolderAsync(CancellationToken cancellationToken)
        {
            vaultFolder = await FileExplorerService.PickFolderAsync(cancellationToken) as IModifiableFolder;
            DialogViewModel.PrimaryButtonEnabled = await UpdateStatusInternalAsync(cancellationToken);
        }
    }
}
