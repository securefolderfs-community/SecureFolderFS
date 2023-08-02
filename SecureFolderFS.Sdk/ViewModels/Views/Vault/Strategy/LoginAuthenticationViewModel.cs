using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault.Strategy
{
    public sealed partial class LoginAuthenticationViewModel : ObservableObject
    {
        [ObservableProperty] private string? _AuthenticationName;

        [RelayCommand]
        private async Task AuthenticateAsync(CancellationToken cancellationToken)
        {

        }
    }
}
