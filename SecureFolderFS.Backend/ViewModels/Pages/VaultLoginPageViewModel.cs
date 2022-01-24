using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Backend.Models;
using SecureFolderFS.Core.PasswordRequest;
using SecureFolderFS.Core.Routines;

#nullable enable

namespace SecureFolderFS.Backend.ViewModels.Pages
{
    public sealed class VaultLoginPageViewModel : BasePageViewModel
    {
        public NavigationModel NavigationModel { get; }

        private string? _VaultName;
        public string? VaultName
        {
            get => _VaultName;
            set => SetProperty(ref _VaultName, value);
        }

        public IRelayCommand<string> UnlockVaultCommand { get; }

        public VaultLoginPageViewModel(VaultModel vaultModel, NavigationModel navigationModel)
            : base(vaultModel)
        {
            this.NavigationModel = navigationModel;
            this._VaultName = vaultModel.VaultName;

            this.UnlockVaultCommand = new RelayCommand<string?>(UnlockVault);
        }

        private void UnlockVault(string? password)
        {
            if (string.IsNullOrEmpty(password))
            {
                // TODO: Please provide password
                NavigationModel.NavigateToPage(VaultModel, new VaultDashboardPageViewModel(VaultModel));
            }
            else
            {
                var disposablePassword = new DisposablePassword(Encoding.UTF8.GetBytes(password));
            }
        }

        public override void Dispose()
        {
        }
    }
}
