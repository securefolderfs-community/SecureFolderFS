using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Scanners;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Categories
{
    [Inject<IVaultService>]
    [Bindable(true)]
    public sealed partial class HealthWidgetViewModel : BaseWidgetViewModel, IProgress<double>, IProgress<TotalProgres>, IViewable
    {
        private readonly UnlockedVaultViewModel _unlockedVaultViewModel;
        private readonly SynchronizationContext? _context;
        private IVaultHealthModel? _vaultHealthModel;
        private CancellationTokenSource? _cts;

        [ObservableProperty] private string _Title;
        [ObservableProperty] private bool _IsProgressing;
        [ObservableProperty] private double _CurrentProgress;
        [ObservableProperty] private string _LastCheckedText;

        public HealthWidgetViewModel(UnlockedVaultViewModel unlockedVaultViewModel, IWidgetModel widgetModel)
            : base(widgetModel)
        {
            ServiceProvider = DI.Default;
            LastCheckedText = string.Format("LastChecked".ToLocalized(), "Unspecified");
            Title = "HealthNoProblems".ToLocalized();
            _context = SynchronizationContext.Current;
            _cts = new();
            _unlockedVaultViewModel = unlockedVaultViewModel;
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            var contentFolder = await _unlockedVaultViewModel.VaultViewModel.VaultModel.GetContentFolderAsync(cancellationToken);
            var folderScanner = new DeepFolderScanner(contentFolder);
            _vaultHealthModel = new VaultHealthModel(folderScanner, true);
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
        public void Report(TotalProgres value)
        {
            if (!IsProgressing)
                return;

            _context?.Post(_ =>
            {
                if (value.Total == 0)
                    Title = value.TotalScanned == 0 ? "Collecting items..." : $"Collecting items ({value.TotalScanned})";
                else
                    Title = $"Scanning items ({value.TotalScanned} of {value.Total})";
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

            _ = ScanAsync();
        }

        private async Task ScanAsync()
        {
            if (_vaultHealthModel is null)
                return;

            await _vaultHealthModel.ScanAsync(new(this, this), _cts?.Token ?? default);
            EndScanning();
        }

        [RelayCommand]
        private void CancelScanning()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new();
            EndScanning();
        }

        [RelayCommand]
        private void OpenVaultHealth()
        {
        }

        private void EndScanning()
        {
            if (!IsProgressing)
                return;

            IsProgressing = false;
            CurrentProgress = 0d;
            // TODO: Depending on scan results update the status
            Title = "HealthNoProblems".ToLocalized(); // HealthNoProblems, HealthAttention, HealthProblems
        }
    }
}
