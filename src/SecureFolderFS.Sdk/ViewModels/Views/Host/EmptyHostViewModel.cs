using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Shared.Extensions;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Host
{
    [Inject<IDialogService>, Inject<ISettingsService>]
    public sealed partial class EmptyHostViewModel : BasePageViewModel
    {
        private readonly INavigationService _hostNavigationService;
        private readonly IVaultCollectionModel _vaultCollectionModel;

        public EmptyHostViewModel(INavigationService hostNavigationService, IVaultCollectionModel vaultCollectionModel)
        {
            ServiceProvider = Ioc.Default;
            _hostNavigationService = hostNavigationService;
            _vaultCollectionModel = vaultCollectionModel;
        }

        private async void VaultCollectionModel_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            await _hostNavigationService.TryNavigateAsync(() => new MainHostViewModel(_hostNavigationService, _vaultCollectionModel));
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
            await SettingsService.TrySaveAsync();
        }

        /// <inheritdoc/>
        public override void OnNavigatingTo(NavigationType navigationType)
        {
            _vaultCollectionModel.CollectionChanged += VaultCollectionModel_CollectionChanged;
        }

        /// <inheritdoc/>
        public override void OnNavigatingFrom()
        {
            _vaultCollectionModel.CollectionChanged -= VaultCollectionModel_CollectionChanged;
        }
    }
}
