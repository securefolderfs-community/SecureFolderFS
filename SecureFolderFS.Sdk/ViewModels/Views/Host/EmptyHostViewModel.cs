using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Host
{
    public sealed partial class EmptyHostViewModel : ObservableObject, IRecipient<AddVaultMessage>
    {
        private readonly IVaultCollectionModel _vaultCollectionModel;

        private IDialogService DialogService { get; } = Ioc.Default.GetRequiredService<IDialogService>();

        private ISettingsService SettingsService { get; } = Ioc.Default.GetRequiredService<ISettingsService>();

        public EmptyHostViewModel(IVaultCollectionModel vaultCollectionModel)
        {
            _vaultCollectionModel = vaultCollectionModel;
            WeakReferenceMessenger.Default.Register(this);
        }

        /// <inheritdoc/>
        public void Receive(AddVaultMessage message)
        {
            WeakReferenceMessenger.Default.Send(new RootNavigationMessage(new MainHostViewModel(_vaultCollectionModel))); // TODO(r)
            WeakReferenceMessenger.Default.Unregister<AddVaultMessage>(this);
        }

        [RelayCommand]
        private async Task AddNewVaultAsync()
        {
            await DialogService.ShowDialogAsync(new VaultWizardDialogViewModel(_vaultCollectionModel));
        }

        [RelayCommand]
        private async Task OpenSettingsAsync()
        {
            await DialogService.ShowDialogAsync(SettingsDialogViewModel.Instance);
            await SettingsService.SaveAsync();
        }
    }
}
