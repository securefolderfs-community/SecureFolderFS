using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard
{
    [Inject<IFileExplorerService>, Inject<IVaultService>]
    [Bindable(true)]
    public sealed partial class LocationWizardViewModel : BaseWizardViewModel
    {
        [ObservableProperty] private string? _Message;
        [ObservableProperty] private string? _SelectedLocation;
        [ObservableProperty] private ViewSeverityType _Severity;

        public NewVaultCreationType CreationType { get; }

        public IFolder? SelectedFolder { get; private set; }

        public LocationWizardViewModel(NewVaultCreationType creationType)
        {
            ServiceProvider = DI.Default;
            CreationType = creationType;

            CanCancel = true;
            CanContinue = false;
            CancelText = "Cancel".ToLocalized();
            ContinueText = "Continue".ToLocalized();
            Title = creationType == NewVaultCreationType.AddExisting ? "AddExisting".ToLocalized() : "CreateNew".ToLocalized();
        }

        /// <inheritdoc/>
        public override Task<IResult> TryContinueAsync(CancellationToken cancellationToken)
        {
            if (SelectedFolder is null)
                return Task.FromResult<IResult>(Result.Failure(null));

            return Task.FromResult<IResult>(Result.Success);
        }

        /// <inheritdoc/>
        public override Task<IResult> TryCancelAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IResult>(Result.Success);
        }

        /// <inheritdoc/>
        public override async void OnAppearing()
        {
            CanContinue = await UpdateStatusAsync();
        }

        [RelayCommand]
        private async Task SelectLocationAsync(CancellationToken cancellationToken)
        {
            SelectedFolder = await FileExplorerService.PickFolderAsync(cancellationToken);
            CanContinue = await UpdateStatusAsync(cancellationToken);
        }

        public async Task<bool> UpdateStatusAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (SelectedFolder is null)
                {
                    Severity = ViewSeverityType.Default;
                    Message = "SelectFolderToContinue".ToLocalized();
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
                            Severity = ViewSeverityType.Warning;
                            Message = "SelectedMayNotBeSupported".ToLocalized();
                            return true;
                        }

                        Severity = ViewSeverityType.Error;
                        Message = "SelectedInvalidVault".ToLocalized();
                        return false;
                    }

                    Severity = ViewSeverityType.Success;
                    Message = "SelectedValidVault".ToLocalized();
                    return true;
                }
                else
                {
                    var validationResult = await VaultService.VaultValidator.TryValidateAsync(SelectedFolder, cancellationToken);
                    if (validationResult.Successful || validationResult.Exception is NotSupportedException)
                    {
                        // Check if a valid (or unsupported) vault exists at a specified path
                        Severity = ViewSeverityType.Warning;
                        Message = "SelectedToBeOverwritten".ToLocalized();
                        return true;
                    }

                    Severity = ViewSeverityType.Success;
                    Message = "SelectedWillCreate".ToLocalized();
                    return true;
                }
            }
            finally
            {
                SelectedLocation = SelectedFolder is null ? "SelectedNone".ToLocalized() : SelectedFolder.Name;
            }
        }
    }
}
