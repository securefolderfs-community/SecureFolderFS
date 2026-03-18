using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    public sealed partial class VaultHealthViewModel
    {
        [RelayCommand]
        private async Task StartScanningAsync(string? mode)
        {
            if (_healthModel is null)
                return;

            // Set IsProgressing status
            IsProgressing = true;
            Title = StatusTitle = "Scanning...";

            // Save last scan state
            _savedState.AddMultiple(FoundIssues);
            FoundIssues.Clear();

            // Determine scan mode
            if (mode?.Contains("rescan", StringComparison.OrdinalIgnoreCase) ?? false)
                mode = _lastScanMode;
            else
                _lastScanMode = mode;

            var includeFileContents = mode?.Contains("include_file_contents", StringComparison.OrdinalIgnoreCase) ?? false;

            // Begin scanning
            await Task.Delay(10);
            _ = Task.Run(async () => await ScanAsync(_healthModel, includeFileContents, _cts?.Token ?? CancellationToken.None));
            await Task.Yield();
        }

        [RelayCommand]
        private async Task ScanFolderAsync(string? mode, CancellationToken cancellationToken)
        {
            if (_contentFolder is null)
                return;

            // Prompt the user to pick a folder
            IFolder pickedFolder = await FileExplorerService.PickFolderAsync(null, false);
            if (pickedFolder is null)
                return;

            if (pickedFolder.Id.StartsWith(_unlockedVaultViewModel.StorageRoot.VirtualizedRoot.Id))
            {
                // The folder is in the virtual storage directory. Get the ciphertext equivalent

                // Get the relative plaintext item
                var relativePath = Path.GetRelativePath(_unlockedVaultViewModel.StorageRoot.VirtualizedRoot.Id, pickedFolder.Id);
                var plaintextRelativeFolder = await _unlockedVaultViewModel.StorageRoot.PlaintextRoot.GetItemByRelativePathAsync(relativePath, cancellationToken) as IFolder;
                if (plaintextRelativeFolder is not IWrapper<IFolder> { Inner: { } relativeCiphertextFolder })
                    return;

                pickedFolder = relativeCiphertextFolder;
            }
            else
            {
                // The picked folder is a ciphertext directory. Ensure it's within the vault's content folder

                // Verify the picked folder is within the vault's content folder
                var contentFolderId = _contentFolder.Id.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var pickedFolderId = pickedFolder.Id.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                if (!pickedFolderId.StartsWith(contentFolderId, StringComparison.OrdinalIgnoreCase))
                    return;
            }

            // Set IsProgressing status
            IsProgressing = true;
            Title = StatusTitle = "Scanning...";

            // Save last scan state
            _savedState.AddMultiple(FoundIssues);
            FoundIssues.Clear();

            // Store scan mode
            _lastScanMode = mode;

            var includeFileContents = mode?.Contains("include_file_contents", StringComparison.OrdinalIgnoreCase) ?? false;

            // Create a health model for the specific folder
            var folderHealthModel = CreateHealthModel(pickedFolder);

            // Begin scanning
            await Task.Delay(10);
            _ = Task.Run(async () =>
            {
                try
                {
                    await ScanAsync(folderHealthModel, includeFileContents, _cts?.Token ?? default);
                }
                finally
                {
                    folderHealthModel.Dispose();
                }
            });
            await Task.Yield();
        }

        [RelayCommand]
        private void CancelScanning()
        {
            // Restore last scan state
            FoundIssues.AddMultiple(_savedState);

            _cts?.CancelAsync();
            _cts?.Dispose();
            _cts = new();
            EndScanning();
            StateChanged?.Invoke(this, new ScanningFinishedEventArgs(true));
        }

        private async Task ScanAsync(IHealthModel healthModel, bool includeFileContents, CancellationToken cancellationToken)
        {
            // Begin scanning
            StateChanged?.Invoke(this, new ScanningStartedEventArgs());
            await healthModel.ScanAsync(includeFileContents, cancellationToken).ConfigureAwait(false);

            // Finish scanning
            EndScanning();
            StateChanged?.Invoke(this, new ScanningFinishedEventArgs(false));
        }

        private void EndScanning()
        {
            if (!IsProgressing)
                return;

            _context.PostOrExecute(_ =>
            {
                // Reset progress
                IsProgressing = false;
                CurrentProgress = 0d;
                _savedState.Clear();

                // Update status
                Title = Severity switch
                {
                    Severity.Warning => "HealthAttention".ToLocalized(),
                    Severity.Critical => "HealthProblems".ToLocalized(),
                    _ => "HealthNoProblems".ToLocalized()
                };
                StatusTitle = "Perform integrity check";
                Subtitle = null;
            });
        }
    }
}
