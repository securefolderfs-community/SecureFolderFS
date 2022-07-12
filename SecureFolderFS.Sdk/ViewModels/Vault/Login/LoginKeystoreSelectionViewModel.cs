using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Sdk.ViewModels.Vault.Login
{
    public sealed class LoginKeystoreSelectionViewModel : ObservableObject
    {
        private IDialogService DialogService { get; } = Ioc.Default.GetRequiredService<IDialogService>();

        public IAsyncRelayCommand SelectKeystoreCommand { get; }

        public LoginKeystoreSelectionViewModel()
        {
            SelectKeystoreCommand = new AsyncRelayCommand(SelectKeystoreAsync);
        }

        private Task SelectKeystoreAsync()
        {
            return Task.CompletedTask;
        }
    }
}