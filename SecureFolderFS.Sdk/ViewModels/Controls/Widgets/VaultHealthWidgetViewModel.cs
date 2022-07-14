using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Widgets
{
    public sealed class VaultHealthWidgetViewModel : BaseWidgetViewModel
    {
        private DateTime _VaultHealthLastCheckedDate;
        public DateTime VaultHealthLastCheckedDate
        {
            get => _VaultHealthLastCheckedDate;
            set => SetProperty(ref _VaultHealthLastCheckedDate, value);
        }

        private VaultHealthState _VaultHealthState;
        public VaultHealthState VaultHealthState
        {
            get => _VaultHealthState;
            set => SetProperty(ref _VaultHealthState, value);
        }

        public IRelayCommand StartScanningCommand { get; }

        public IRelayCommand OpenVaultHealthCommand { get; }

        public VaultHealthWidgetViewModel(IWidgetsContextModel widgetsContextModel)
            : base(widgetsContextModel)
        {
            StartScanningCommand = new RelayCommand(StartScanning);
            OpenVaultHealthCommand = new RelayCommand(OpenVaultHealth);
        }

        private void StartScanning()
        {

        }

        private void OpenVaultHealth()
        {

        }

        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
