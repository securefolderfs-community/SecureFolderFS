using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.Scanners;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Health
{
    [Inject<ILocalizationService>, Inject<IVaultFileSystemService>]
    [Bindable(true)]
    public sealed partial class HealthWidgetViewModel : BaseWidgetViewModel, IProgress<double>, IProgress<TotalProgress>, IViewable
    {
        private readonly UnlockedVaultViewModel _unlockedVaultViewModel;
        private readonly List<HealthIssueViewModel> _savedState;
        private readonly SynchronizationContext? _context;
        private readonly INavigator _dashboardNavigator;
        private IHealthModel? _vaultHealthModel;
        private CancellationTokenSource? _cts;
        private string? _lastScanMode;

        [ObservableProperty] private string? _Title;
        [ObservableProperty] private string? _LastCheckedText;
        [ObservableProperty] private VaultHealthReportViewModel _HealthReportViewModel;

        public HealthWidgetViewModel(UnlockedVaultViewModel unlockedVaultViewModel, INavigator dashboardNavigator, IWidgetModel widgetModel)
            : base(widgetModel)
        {
            ServiceProvider = DI.Default;
            _cts = new();
            _savedState = new();
            _context = SynchronizationContext.Current;
            _dashboardNavigator = dashboardNavigator;
            _unlockedVaultViewModel = unlockedVaultViewModel;
            HealthReportViewModel = new(unlockedVaultViewModel, _context) { StartScanningCommand = StartScanningCommand };
            LastCheckedText = string.Format("LastChecked".ToLocalized(), "Unspecified");
            Title = "HealthNoProblems".ToLocalized();
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            var vaultModel = _unlockedVaultViewModel.VaultViewModel.VaultModel;
            var contentFolder = await vaultModel.GetContentFolderAsync(cancellationToken);
            var folderScanner = new DeepFolderScanner(contentFolder);
            var structureValidator = _unlockedVaultViewModel.StorageRoot.Options.HealthStatistics.StructureValidator;

            if (_vaultHealthModel is not null)
            {
                _vaultHealthModel.IssueFound -= VaultHealthModel_IssueFound;
                _vaultHealthModel.Dispose();
            }
            _vaultHealthModel = new HealthModel(folderScanner, new(this, this), structureValidator);
            _vaultHealthModel.IssueFound += VaultHealthModel_IssueFound;

            // Get persisted last scanned date
            var rawLastScanDate = await WidgetModel.GetWidgetDataAsync(cancellationToken);
            if (rawLastScanDate is null)
                return;

            if (!DateTime.TryParse(rawLastScanDate, out var lastScanDate))
                return;

            var localizedDate = LocalizationService.LocalizeDate(lastScanDate);
            LastCheckedText = string.Format("LastChecked".ToLocalized(), localizedDate);
        }

        /// <inheritdoc/>
        public void Report(double value)
        {
            if (!HealthReportViewModel.IsProgressing)
                return;

            _context.PostOrExecute(_ => HealthReportViewModel.CurrentProgress = Math.Round(value));
        }

        /// <inheritdoc/>
        public void Report(TotalProgress value)
        {
            if (!HealthReportViewModel.IsProgressing)
                return;

            _context.PostOrExecute(_ =>
            {
                if (value.Total == 0)
                    Title = value.Achieved == 0
                        ? "Collecting items..."
                        : $"Collecting items ({value.Achieved})";
                else
                    Title = value.Achieved == value.Total
                            ? "Scan completed"
                            : $"Scanning items ({value.Achieved} of {value.Total})";
            });
        }

        [RelayCommand]
        private async Task OpenVaultHealthAsync()
        {
            await _dashboardNavigator.NavigateAsync(HealthReportViewModel);
        }

        [RelayCommand]
        private async Task StartScanningAsync(string? mode)
        {
            if (_vaultHealthModel is null)
                return;

            // Set IsProgressing status
            HealthReportViewModel.IsProgressing = true;
            Title = "Scanning...";

            // Save last scan state
            _savedState.AddMultiple(HealthReportViewModel.FoundIssues);
            HealthReportViewModel.FoundIssues.Clear();

            // Determine scan mode
            if (mode?.Contains("rescan", StringComparison.OrdinalIgnoreCase) ?? false)
                mode = _lastScanMode;
            else
                _lastScanMode = mode;

            var includeFileContents = mode?.Contains("include_file_contents", StringComparison.OrdinalIgnoreCase) ?? false;

            // Begin scanning
            await Task.Delay(10);
            _ = Task.Run(() => ScanAsync(includeFileContents, _cts?.Token ?? default));
            await Task.Yield();
        }

        [RelayCommand]
        private void CancelScanning()
        {
            // Restore last scan state
            HealthReportViewModel.FoundIssues.AddMultiple(_savedState);

            _cts?.CancelAsync();
            _cts?.Dispose();
            _cts = new();
            EndScanning();
            _context.PostOrExecute(_ => HealthReportViewModel.CanResolve = false);
        }

        private async Task ScanAsync(bool includeFileContents, CancellationToken cancellationToken)
        {
            if (_vaultHealthModel is null)
                return;

            // Begin scanning
            _context.PostOrExecute(_ => HealthReportViewModel.CanResolve = false);
            await _vaultHealthModel.ScanAsync(includeFileContents, cancellationToken).ConfigureAwait(false);
            EndScanning();

            // Finish scanning
            var scanDate = DateTime.Now;
            _context.PostOrExecute(_ =>
            {
                HealthReportViewModel.CanResolve = !HealthReportViewModel.FoundIssues.IsEmpty();
                var localizedDate = LocalizationService.LocalizeDate(scanDate);
                LastCheckedText = string.Format("LastChecked".ToLocalized(), localizedDate);
            });

            // Persist last scanned date
            await WidgetModel.SetWidgetDataAsync(scanDate.ToString("o"), cancellationToken);
        }

        private void EndScanning()
        {
            if (!HealthReportViewModel.IsProgressing)
                return;

            _context.PostOrExecute(_ =>
            {
                // Reset progress
                HealthReportViewModel.IsProgressing = false;
                HealthReportViewModel.CurrentProgress = 0d;
                _savedState.Clear();

                // Update status
                Title = HealthReportViewModel.Severity switch
                {
                    SeverityType.Warning => "HealthAttention".ToLocalized(),
                    SeverityType.Critical => "HealthProblems".ToLocalized(),
                    _ => "HealthNoProblems".ToLocalized()
                };
            });
        }

        private async void VaultHealthModel_IssueFound(object? sender, HealthIssueEventArgs e)
        {
            if (e.Result.Successful || e.Storable is null)
                return;

            var issueViewModel = await VaultFileSystemService.GetIssueViewModelAsync(e.Result, e.Storable);
            _context.PostOrExecute(_ => HealthReportViewModel.FoundIssues.Add(issueViewModel ?? new(e.Storable, e.Result)));
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            if (_vaultHealthModel is not null)
            {
                _vaultHealthModel.IssueFound -= VaultHealthModel_IssueFound;
                _vaultHealthModel.Dispose();
            }

            HealthReportViewModel.Dispose();
            base.Dispose();
        }
    }
}
