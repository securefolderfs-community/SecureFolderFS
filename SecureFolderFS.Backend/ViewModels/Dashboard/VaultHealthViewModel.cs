using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Backend.Enums;

#nullable enable

namespace SecureFolderFS.Backend.ViewModels.Dashboard
{
    public sealed class VaultHealthViewModel : ObservableObject
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

        public VaultHealthViewModel()
        {
            this.StartScanningCommand = new RelayCommand(StartScanning);
            this.OpenVaultHealthCommand = new RelayCommand(OpenVaultHealth);
        }

        private void StartScanning()
        {

        }

        private void OpenVaultHealth()
        {

        }
    }
}
