using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets
{
    public sealed partial class VaultHealthWidgetViewModel : BaseWidgetViewModel
    {
        [ObservableProperty] private string _LastCheckedText;
        [ObservableProperty] private VaultHealthState _VaultHealthState;

        public VaultHealthWidgetViewModel(IWidgetModel widgetModel)
            : base(widgetModel)
        {
            LastCheckedText = string.Format("LastChecked".ToLocalized(), "Unspecified");
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
