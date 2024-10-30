using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.Scanners;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Categories
{
    [Inject<ILocalizationService>, Inject<IImageService>]
    [Bindable(true)]
    public sealed partial class HealthWidgetViewModel : BaseWidgetViewModel, IProgress<double>, IProgress<TotalProgress>, IViewable
    {
        private readonly UnlockedVaultViewModel _unlockedVaultViewModel;
        private readonly SynchronizationContext? _context;
        private IHealthModel? _vaultHealthModel;
        private CancellationTokenSource? _cts;

        [ObservableProperty] private string? _Title;
        [ObservableProperty] private IImage? _StatusIcon;
        [ObservableProperty] private bool _IsProgressing;
        [ObservableProperty] private double _CurrentProgress;
        [ObservableProperty] private string? _LastCheckedText;
        [ObservableProperty] private ObservableCollection<string> _FoundIssues; // TODO: (1) Add a view model type. (2) Move to a separate VM e.g. HealthDetailsViewModel

        public HealthWidgetViewModel(UnlockedVaultViewModel unlockedVaultViewModel, IWidgetModel widgetModel)
            : base(widgetModel)
        {
            ServiceProvider = DI.Default;
            FoundIssues = new();
            Title = "HealthNoProblems".ToLocalized();
            LastCheckedText = string.Format("LastChecked".ToLocalized(), "Unspecified");
            _cts = new();
            _context = SynchronizationContext.Current;
            _unlockedVaultViewModel = unlockedVaultViewModel;
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

            _context?.Post(_ =>
            {
                CurrentProgress = Math.Round(value);
            }, null);
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
        private async Task StartScanningAsync()
        {
            if (_vaultHealthModel is null)
                return;

            // Set IsProgressing with small delay
            IsProgressing = true;
            Title = "Scanning...";
            await Task.Delay(10);

            _ = ScanAsync(_cts?.Token ?? default);
            await Task.Yield();
        }

        [RelayCommand]
        private async Task CancelScanningAsync()
        {
            _cts?.CancelAsync();
            _cts?.Dispose();
            _cts = new();
            await EndScanningAsync();
        }

        [RelayCommand]
        private void OpenVaultHealth()
        {
        }

        private async Task ScanAsync(CancellationToken cancellationToken)
        {
            if (_vaultHealthModel is null)
                return;

            await _vaultHealthModel.ScanAsync(new(this, this), cancellationToken);
            await EndScanningAsync();
        }

        private async Task EndScanningAsync()
        {
            if (!IsProgressing)
                return;

            // Reset progress
            IsProgressing = false;
            CurrentProgress = 0d;

            // Update date TODO: Persist last checked date
            var localizedDate = LocalizationService.LocalizeDate(DateTime.Now);
            LastCheckedText = string.Format("LastChecked".ToLocalized(), localizedDate);
            
            // Update status TODO: Update icon
            if (FoundIssues.IsEmpty())
            {
                Title = "HealthNoProblems".ToLocalized(); // HealthNoProblems, HealthAttention, HealthProblems
                StatusIcon = await ImageService.GetHealthIconAsync(SeverityType.Success);
            }
            else
            {
                Title = "HealthProblems".ToLocalized();
                StatusIcon = await ImageService.GetHealthIconAsync(SeverityType.Error);
            }
        }

        private void VaultHealthModel_IssueFound(object? sender, HealthIssueEventArgs e)
        {
            FoundIssues.Add(e.Result.GetMessage());
            _ = e;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            if (_vaultHealthModel is not null)
            {
                _vaultHealthModel.IssueFound -= VaultHealthModel_IssueFound;
                _vaultHealthModel.Dispose();
            }

            base.Dispose();
        }
    }
}
