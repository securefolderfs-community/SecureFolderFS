using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard.ExistingVault
{
    [Inject<IVaultService>]
    public sealed partial class ExistingLocationWizardViewModel : BaseVaultSelectionWizardViewModel
    {
        public ExistingLocationWizardViewModel(VaultWizardDialogViewModel dialogViewModel)
            : base(dialogViewModel)
        {
            ServiceProvider = Ioc.Default;
        }

        /// <inheritdoc/>
        public override async Task PrimaryButtonClickAsync(IEventDispatch? eventDispatch, CancellationToken cancellationToken)
        {
            eventDispatch?.NoForwarding();
            if (vaultFolder is null)
                return;

            var vaultModel = new VaultModel(vaultFolder);
            DialogViewModel.VaultCollectionModel.Add(vaultModel);
            await DialogViewModel.VaultCollectionModel.TrySaveAsync(cancellationToken);

            await NavigationService.TryNavigateAsync(() => new SummaryWizardViewModel(vaultModel.VaultName, DialogViewModel));
        }

        /// <inheritdoc/>
        protected override async Task<bool> UpdateStatusAsync(CancellationToken cancellationToken)
        {
            if (vaultFolder is null)
                return false;

            var validationResult = await VaultService.VaultValidator.TryValidateAsync(vaultFolder, cancellationToken);
            if (!validationResult.Successful)
            {
                if (validationResult.Exception is NotSupportedException)
                {
                    // Allow unsupported vaults to be migrated
                    SelectionInfoBar.Severity = InfoBarSeverityType.Warning;
                    SelectionInfoBar.Message = "Selected vault may not be supported";
                    return true;
                }
                else
                {
                    SelectionInfoBar.Severity = InfoBarSeverityType.Error;
                    SelectionInfoBar.Message = "Vault folder is invalid";
                    return false;
                }
            }

            SelectionInfoBar.Severity = InfoBarSeverityType.Success;
            SelectionInfoBar.Message = "Found a valid vault folder";
            return true;
        }
    }
}
