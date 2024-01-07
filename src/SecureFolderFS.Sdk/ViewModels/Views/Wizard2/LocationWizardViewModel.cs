using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard2
{
    [Inject<IFileExplorerService>, Inject<IVaultService>]
    public sealed partial class LocationWizardViewModel : BaseWizardViewModel
    {
        private readonly IVaultCollectionModel _vaultCollectionModel;

        [ObservableProperty] private string? _Message;
        [ObservableProperty] private string? _SelectedLocation;

        public NewVaultCreationType CreationType { get; }

        public IFolder? SelectedFolder { get; private set; }

        public LocationWizardViewModel(NewVaultCreationType creationType, IVaultCollectionModel vaultCollectionModel)
        {
            ServiceProvider = Ioc.Default;
            CreationType = creationType;
            _vaultCollectionModel = vaultCollectionModel;
            CanContinue = false;
            CanCancel = true;
        }

        /// <inheritdoc/>
        public override async Task<IResult> TryContinueAsync(CancellationToken cancellationToken)
        {
            if (SelectedFolder is null)
                return CommonResult.Failure(null);

            if (CreationType == NewVaultCreationType.AddExisting)
            {
                // Add the newly created vault
                var vaultModel = new VaultModel(SelectedFolder);
                _vaultCollectionModel.Add(vaultModel);

                // Try to save the new vault
                await _vaultCollectionModel.TrySaveAsync(cancellationToken);
            }

            return CommonResult.Success;
        }

        /// <inheritdoc/>
        public override Task<IResult> TryCancelAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IResult>(CommonResult.Success);
        }

        [RelayCommand]
        private async Task SelectLocationAsync(CancellationToken cancellationToken)
        {
            SelectedFolder = await FileExplorerService.PickFolderAsync(cancellationToken);
            CanContinue = await UpdateStatusAsync(cancellationToken);
        }

        private async Task<bool> UpdateStatusAsync(CancellationToken cancellationToken)
        {
            if (SelectedFolder is null)
            {
                //SelectionInfoBar.Severity = InfoBarSeverityType.Information;
                Message = "Select a folder to continue";
                SelectedLocation = "No vault selected";
                return false;
            }

            if (CreationType == NewVaultCreationType.AddExisting)
            {
                var validationResult = await VaultService.VaultValidator.TryValidateAsync(SelectedFolder, cancellationToken);
                if (!validationResult.Successful)
                {
                    if (validationResult.Exception is NotSupportedException)
                    {
                        // Allow unsupported vaults to be migrated
                        //SelectionInfoBar.Severity = InfoBarSeverityType.Warning;
                        Message = "Selected vault may not be supported";
                        return true;
                    }

                    //SelectionInfoBar.Severity = InfoBarSeverityType.Error;
                    Message = "Vault folder is invalid";
                    return false;
                }

                //SelectionInfoBar.Severity = InfoBarSeverityType.Success;
                Message = "Found a valid vault folder";
                return true;
            }
            else
            {
                var validationResult = await VaultService.VaultValidator.TryValidateAsync(SelectedFolder, cancellationToken);
                if (validationResult.Successful || validationResult.Exception is NotSupportedException)
                {
                    // Check if a valid (or unsupported) vault exists at a specified path
                    //SelectionInfoBar.Severity = InfoBarSeverityType.Warning;
                    Message = "The selected vault will be overwritten";
                    return true;
                }

                //SelectionInfoBar.Severity = InfoBarSeverityType.Success;
                Message = "A new vault will be created in selected folder";
                return true;
            }
        }
    }
}
