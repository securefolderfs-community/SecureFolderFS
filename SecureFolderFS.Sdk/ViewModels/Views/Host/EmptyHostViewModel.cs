using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Host
{
    public sealed partial class EmptyHostViewModel : BasePageViewModel, IRecipient<AddVaultMessage>
    {
        private readonly INavigationService _hostNavigationService;
        private readonly IVaultCollectionModel _vaultCollectionModel;

        private IDialogService DialogService { get; } = Ioc.Default.GetRequiredService<IDialogService>();

        private ISettingsService SettingsService { get; } = Ioc.Default.GetRequiredService<ISettingsService>();

        public EmptyHostViewModel(INavigationService hostNavigationService, IVaultCollectionModel vaultCollectionModel)
        {
            _hostNavigationService = hostNavigationService;
            _vaultCollectionModel = vaultCollectionModel;
            WeakReferenceMessenger.Default.Register(this);
        }

        /// <inheritdoc/>
        public async void Receive(AddVaultMessage message) // TODO: Remove when IVaultCollectionModel inherits from ICollection with the ability to listen for changes
        {
            // Event handler will not be unhooked here since the view model is cached anyway
            await _hostNavigationService.TryNavigateAsync(() => new MainHostViewModel(_hostNavigationService, _vaultCollectionModel));
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
