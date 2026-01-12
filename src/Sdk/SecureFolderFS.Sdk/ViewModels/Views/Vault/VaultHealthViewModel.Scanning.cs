using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;

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
            _ = Task.Run(async () => await ScanAsync(_healthModel, includeFileContents, _cts?.Token ?? default));
            await Task.Yield();
        }

        [RelayCommand]
        private async Task ScanFolderAsync(string? mode)
        {
            if (_contentFolder is null)
                return;

            // Prompt user to pick a folder
            var pickedFolder = await FileExplorerService.PickFolderAsync(null, false);
            if (pickedFolder is null)
                return;

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
                    Enums.Severity.Warning => "HealthAttention".ToLocalized(),
                    Enums.Severity.Critical => "HealthProblems".ToLocalized(),
                    _ => "HealthNoProblems".ToLocalized()
                };
                StatusTitle = "Perform integrity check";
                Subtitle = null;
            });
        }
    }
}
