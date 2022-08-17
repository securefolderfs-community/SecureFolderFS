using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Dialogs;

namespace SecureFolderFS.Sdk.ViewModels.AppHost
{
    public sealed partial class NoVaultsAppHostViewModel : ObservableObject, IRecipient<AddVaultMessage>
    {
        private readonly IVaultCollectionModel _vaultCollectionModel;

        private IDialogService DialogService { get; } = Ioc.Default.GetRequiredService<IDialogService>();

        public NoVaultsAppHostViewModel(IVaultCollectionModel vaultCollectionModel)
        {
            _vaultCollectionModel = vaultCollectionModel;
            WeakReferenceMessenger.Default.Register(this);
        }

        /// <inheritdoc/>
        public async void Receive(AddVaultMessage message)
        {
            await _vaultCollectionModel.AddVaultAsync(message.VaultModel);

            WeakReferenceMessenger.Default.Send(new RootNavigationRequestedMessage(new MainAppHostViewModel(_vaultCollectionModel)));
            WeakReferenceMessenger.Default.Unregister<AddVaultMessage>(this);
        }

        [RelayCommand]
        private async Task AddNewVaultAsync()
        {
            await DialogService.ShowDialogAsync(new VaultWizardDialogViewModel());
        }
    }
}
