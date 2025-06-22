using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Health
{
    [Inject<ILocalizationService>, Inject<IVaultHealthService>]
    [Bindable(true)]
    public sealed partial class HealthWidgetViewModel : BaseWidgetViewModel
    {
        private readonly SynchronizationContext? _context;
        private readonly INavigator _dashboardNavigator;

        [ObservableProperty] private string? _LastCheckedText;
        [ObservableProperty] private VaultHealthViewModel _HealthViewModel;
        [ObservableProperty] private VaultHealthReportViewModel _HealthReportViewModel;

        public HealthWidgetViewModel(UnlockedVaultViewModel unlockedVaultViewModel, INavigator dashboardNavigator, IWidgetModel widgetModel)
            : base(widgetModel)
        {
            ServiceProvider = DI.Default;
            _context = SynchronizationContext.Current;
            _dashboardNavigator = dashboardNavigator;
            HealthViewModel = new(unlockedVaultViewModel);
            HealthReportViewModel = new(unlockedVaultViewModel, HealthViewModel);
            LastCheckedText = string.Format("LastChecked".ToLocalized(), "Unspecified");
            HealthViewModel.StateChanged += HealthViewModel_StateChanged;
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            // Initialize HealthViewModel
            await HealthViewModel.InitAsync(cancellationToken);

            // Get persisted last scanned date
            var rawLastScanDate = await WidgetModel.GetWidgetDataAsync(cancellationToken);
            if (rawLastScanDate is null)
                return;

            if (!DateTime.TryParse(rawLastScanDate, out var lastScanDate))
                return;

            var localizedDate = LocalizationService.LocalizeDate(lastScanDate);
            LastCheckedText = string.Format("LastChecked".ToLocalized(), localizedDate);
        }

        [RelayCommand]
        private async Task OpenVaultHealthAsync()
        {
            await _dashboardNavigator.NavigateAsync(HealthReportViewModel);
        }

        private async void HealthViewModel_StateChanged(object? sender, EventArgs e)
        {
            if (e is not ScanningFinishedEventArgs)
                return;

            var scanDate = DateTime.Now;
            _context.PostOrExecute(_ =>
            {
                var localizedDate = LocalizationService.LocalizeDate(scanDate);
                LastCheckedText = string.Format("LastChecked".ToLocalized(), localizedDate);
            });

            // Persist last scanned date
            await WidgetModel.SetWidgetDataAsync(scanDate.ToString("o")).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            HealthViewModel.StateChanged -= HealthViewModel_StateChanged;
            HealthViewModel.Dispose();
            HealthReportViewModel.Dispose();
            base.Dispose();
        }
    }
}
