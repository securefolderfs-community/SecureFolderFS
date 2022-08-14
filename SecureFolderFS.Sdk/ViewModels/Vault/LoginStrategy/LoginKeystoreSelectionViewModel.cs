using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Sdk.ViewModels.Vault.LoginStrategy
{
    public sealed partial class LoginKeystoreSelectionViewModel : BaseLoginStrategyViewModel
    {
        private IDialogService DialogService { get; } = Ioc.Default.GetRequiredService<IDialogService>();

        [RelayCommand]
        private Task SelectKeystoreAsync()
        {
            return Task.CompletedTask;
        }
    }
}