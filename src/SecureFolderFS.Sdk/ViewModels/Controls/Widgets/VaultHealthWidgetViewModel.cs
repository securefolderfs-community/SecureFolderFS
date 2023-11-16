using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets
{
    public sealed partial class VaultHealthWidgetViewModel : BaseWidgetViewModel
    {
        [ObservableProperty] private DateTime _VaultHealthLastCheckedDate;
        [ObservableProperty] private VaultHealthState _VaultHealthState;

        public VaultHealthWidgetViewModel(IWidgetModel widgetModel)
            : base(widgetModel)
        {
        }

        [RelayCommand]
        private Task StartScanningAsync()
        {
            return Task.CompletedTask;
        }

        [RelayCommand]
        private void OpenVaultHealth()
        {
        }

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
