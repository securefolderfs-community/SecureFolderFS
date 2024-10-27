using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.Scanners;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Categories
{
    [Inject<IVaultService>]
    [Bindable(true)]
    public sealed partial class HealthWidgetViewModel : BaseWidgetViewModel, IProgress<double>, IProgress<IResult>, IViewable
    {
        private readonly UnlockedVaultViewModel _unlockedVaultViewModel;
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
            Title = "HealthNoProblems".ToLocalized(); // HealthNoProblems, HealthAttention, HealthProblems
            _cts = new();
            _unlockedVaultViewModel = unlockedVaultViewModel;
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            var contentFolder = await _unlockedVaultViewModel.VaultViewModel.VaultModel.GetContentFolderAsync(cancellationToken);
            var folderScanner = new DeepFolderScanner(contentFolder);
            _vaultHealthModel = new VaultHealthModel(folderScanner);
        }

        /// <inheritdoc/>
        public void Report(double value)
        {
            IsProgressing = true;
            CurrentProgress = value;
        }

        /// <inheritdoc/>
        public void Report(IResult value)
        {
            Title = value.GetMessage();
        }

        [RelayCommand]
        private void StartScanning()
        {
            ArgumentNullException.ThrowIfNull(_vaultHealthModel);

            IsProgressing = true;
            _ = _vaultHealthModel.ScanAsync(new(this, this), _cts?.Token ?? default).ContinueWith(x =>
            {
                IsProgressing = false;
            });
        }

        [RelayCommand]
        private void CancelScanning()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new();
            IsProgressing = false;
        }

        [RelayCommand]
        private void OpenVaultHealth()
        {
        }
    }
}
