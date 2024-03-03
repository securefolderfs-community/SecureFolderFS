using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard
{
    [Inject<IFileExplorerService>, Inject<IVaultService>]
    public sealed partial class LocationWizardViewModel : BaseWizardViewModel
    {
        [ObservableProperty] private string? _Message;
        [ObservableProperty] private string? _SelectedLocation;
        [ObservableProperty] private ViewSeverityType _Severity;

        public NewVaultCreationType CreationType { get; }

        public IFolder? SelectedFolder { get; private set; }

        public LocationWizardViewModel(NewVaultCreationType creationType)
        {
            ServiceProvider = Ioc.Default;
            // TODO: Add title
            CanContinue = false;
            CanCancel = true;
            CreationType = creationType;
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
                    Message = "Select a folder to continue";
                    return false;
                }

                if (CreationType == NewVaultCreationType.AddExisting)
                {
                    var validationResult =
                        await VaultService.VaultValidator.TryValidateAsync(SelectedFolder, cancellationToken);
                    if (!validationResult.Successful)
                    {
                        if (validationResult.Exception is NotSupportedException)
                        {
                            // Allow unsupported vaults to be migrated
                            Severity = ViewSeverityType.Warning;
                            Message = "Selected vault may not be supported";
                            return true;
                        }

                        Severity = ViewSeverityType.Error;
                        Message = "Vault folder is invalid";
                        return false;
                    }

                    Severity = ViewSeverityType.Success;
                    Message = "Found a valid vault folder";
                    return true;
                }
                else
                {
                    var validationResult =
                        await VaultService.VaultValidator.TryValidateAsync(SelectedFolder, cancellationToken);
                    if (validationResult.Successful || validationResult.Exception is NotSupportedException)
                    {
                        // Check if a valid (or unsupported) vault exists at a specified path
                        Severity = ViewSeverityType.Warning;
                        Message = "The selected vault will be overwritten";
                        return true;
                    }

                    Severity = ViewSeverityType.Success;
                    Message = "A new vault will be created in selected folder";
                    return true;
                }
            }
            finally
            {
                if (SelectedFolder is null)
                    SelectedLocation = "No vault selected";
                else
                {
                    SelectedLocation = $"..{Path.DirectorySeparatorChar}{Path.GetFileName(Path.GetDirectoryName(SelectedFolder.Id))}{Path.DirectorySeparatorChar}{SelectedFolder.Name}";
                }
            }
        }
    }
}
