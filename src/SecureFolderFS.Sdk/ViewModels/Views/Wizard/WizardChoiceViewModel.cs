using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.Extensions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard
{
    [Inject<IFileExplorerService>, Inject<IVaultService>]
    public sealed partial class WizardChoiceViewModel : ObservableObject
    {
        private readonly NewVaultCreationType _creationType;
        private readonly DialogViewModel _dialogViewModel;

        [ObservableProperty] private InfoBarViewModel _SelectionInfoBar;
        [ObservableProperty] private string? _SelectedLocation;

        public IFolder? VaultFolder { get; private set; }

        public WizardChoiceViewModel(NewVaultCreationType creationType, DialogViewModel dialogViewModel)
        {
            ServiceProvider = Ioc.Default;
            SelectionInfoBar = new();
            _creationType = creationType;
            _dialogViewModel = dialogViewModel;
        }

        [RelayCommand]
        private async Task OpenFolderAsync(CancellationToken cancellationToken)
        {
            VaultFolder = await FileExplorerService.PickFolderAsync(cancellationToken);
            _dialogViewModel.PrimaryButtonEnabled = await UpdateStatusAsync(cancellationToken);

            if (VaultFolder is not null)
                SelectedLocation = $"..{Path.DirectorySeparatorChar}{Path.GetFileName(Path.GetDirectoryName(VaultFolder.Id))}{Path.DirectorySeparatorChar}{Path.GetFileName(VaultFolder.Id)}";
        }

        public async Task<bool> UpdateStatusAsync(CancellationToken cancellationToken)
        {
            if (VaultFolder is null)
            {
                SelectionInfoBar.Severity = InfoBarSeverityType.Information;
                SelectionInfoBar.Message = "Select a folder to continue";
                SelectedLocation = "No vault selected";
                return false;
            }

            if (_creationType == NewVaultCreationType.AddExisting)
            {
                var validationResult = await VaultService.VaultValidator.TryValidateAsync(VaultFolder, cancellationToken);
                if (!validationResult.Successful)
                {
                    if (validationResult.Exception is NotSupportedException)
                    {
                        // Allow unsupported vaults to be migrated
                        SelectionInfoBar.Severity = InfoBarSeverityType.Warning;
                        SelectionInfoBar.Message = "Selected vault may not be supported";
                        return true;
                    }

                    SelectionInfoBar.Severity = InfoBarSeverityType.Error;
                    SelectionInfoBar.Message = "Vault folder is invalid";
                    return false;
                }

                SelectionInfoBar.Severity = InfoBarSeverityType.Success;
                SelectionInfoBar.Message = "Found a valid vault folder";
                return true;
            }
            else
            {
                var validationResult = await VaultService.VaultValidator.TryValidateAsync(VaultFolder, cancellationToken);
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
        }
    }
}
