using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Results;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.Scanners;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Categories
{
    [Inject<ILocalizationService>, Inject<IOverlayService>]
    [Bindable(true)]
    public sealed partial class HealthWidgetViewModel : BaseWidgetViewModel, IProgress<double>, IProgress<TotalProgress>, IViewable
    {
        private readonly UnlockedVaultViewModel _unlockedVaultViewModel;
        private readonly List<HealthIssueViewModel> _savedState;
        private readonly SynchronizationContext? _context;
        private IHealthModel? _vaultHealthModel;
        private CancellationTokenSource? _cts;

        [ObservableProperty] private string? _Title;
        [ObservableProperty] private bool _IsProgressing;
        [ObservableProperty] private double _CurrentProgress;
        [ObservableProperty] private string? _LastCheckedText;
        [ObservableProperty] private HealthOverlayViewModel _HealthOverlayViewModel;

        public HealthWidgetViewModel(UnlockedVaultViewModel unlockedVaultViewModel, IWidgetModel widgetModel)
            : base(widgetModel)
        {
            ServiceProvider = DI.Default;
            _cts = new();
            _savedState = new();
            _context = SynchronizationContext.Current;
            _unlockedVaultViewModel = unlockedVaultViewModel;
            HealthOverlayViewModel = new(_context);
            Title = "HealthNoProblems".ToLocalized();
            LastCheckedText = string.Format("LastChecked".ToLocalized(), "Unspecified");
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            var vaultModel = _unlockedVaultViewModel.VaultViewModel.VaultModel;
            var contentFolder = await vaultModel.GetContentFolderAsync(cancellationToken);
            var folderScanner = new DeepFolderScanner(contentFolder);
            var fileValidator = _unlockedVaultViewModel.StorageRoot.HealthStatistics.FileValidator;
            var folderValidator = _unlockedVaultViewModel.StorageRoot.HealthStatistics.FolderValidator;

            _vaultHealthModel = new HealthModel(folderScanner, fileValidator, folderValidator);
            _vaultHealthModel.IssueFound += VaultHealthModel_IssueFound;
        }

        /// <inheritdoc/>
        public void Report(double value)
        {
            if (!IsProgressing)
                return;

            _context?.Post(_ => CurrentProgress = Math.Round(value), null);
        }

        /// <inheritdoc/>
        public void Report(TotalProgress value)
        {
            if (!IsProgressing)
                return;

            _context?.Post(_ =>
            {
                if (value.Total == 0)
                    Title = value.Achieved == 0
                        ? "Collecting items..."
                        : $"Collecting items ({value.Achieved})";
                else
                    Title = value.Achieved == value.Total
                            ? "Scan completed"
                            : $"Scanning items ({value.Achieved} of {value.Total})";
            }, null);
        }

        [RelayCommand]
        private async Task OpenVaultHealthAsync()
        {
            await OverlayService.ShowAsync(HealthOverlayViewModel);
        }

        [RelayCommand]
        private async Task StartScanningAsync()
        {
            if (_vaultHealthModel is null)
                return;

            // Set IsProgressing status
            HealthOverlayViewModel.IsScanning = true; // TODO: Move to separate ScanViewModel (name tbd) to avoid duplication of properties
            IsProgressing = true;
            Title = "Scanning...";

            // Save last scan state
            _savedState.AddMultiple(HealthOverlayViewModel.FoundIssues);
            HealthOverlayViewModel.FoundIssues.Clear();

            // Wait a small delay for UI to update
            await Task.Delay(10);

            _ = ScanAsync(_cts?.Token ?? default);
            await Task.Yield();
        }

        [RelayCommand]
        private void CancelScanning()
        {
            // Restore last scan state
            HealthOverlayViewModel.FoundIssues.AddMultiple(_savedState);

            _cts?.CancelAsync();
            _cts?.Dispose();
            _cts = new();
            EndScanning();
        }

        private async Task ScanAsync(CancellationToken cancellationToken)
        {
            if (_vaultHealthModel is null)
                return;

            await _vaultHealthModel.ScanAsync(new(this, this), cancellationToken);
            EndScanning();
        }

        private void EndScanning()
        {
            if (!IsProgressing)
                return;

            // Reset progress
            IsProgressing = false;
            HealthOverlayViewModel.IsScanning = false; // TODO: Move to separate ScanViewModel (name tbd) to avoid duplication of properties
            CurrentProgress = 0d;
            _savedState.Clear();

            // Update date TODO: Persist last checked date
            var localizedDate = LocalizationService.LocalizeDate(DateTime.Now);
            LastCheckedText = string.Format("LastChecked".ToLocalized(), localizedDate);
            
            // Update status
            Title = HealthOverlayViewModel.Severity switch
            {
                SeverityType.Warning => "HealthAttention".ToLocalized(),
                SeverityType.Critical => "HealthProblems".ToLocalized(),
                _ => "HealthNoProblems".ToLocalized()
            };
        }

        private void VaultHealthModel_IssueFound(object? sender, HealthIssueEventArgs e)
        {
            HealthOverlayViewModel.FoundIssues.Add(e.Result switch
            {
                IHealthResult healthResult => new(healthResult),
                _ => new(e.Result, SeverityType.Warning, e.Storable)
            });
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            if (_vaultHealthModel is not null)
            {
                _vaultHealthModel.IssueFound -= VaultHealthModel_IssueFound;
                _vaultHealthModel.Dispose();
            }

            HealthOverlayViewModel.Dispose();
            base.Dispose();
        }
    }
}
