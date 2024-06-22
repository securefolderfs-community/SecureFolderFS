using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Host
{
    [Inject<IOverlayService>, Inject<ISettingsService>]
    [Bindable(true)]
    public sealed partial class EmptyHostViewModel : ObservableObject, IViewDesignation
    {
        private readonly INavigationService _rootNavigationService;
        private readonly IVaultCollectionModel _vaultCollectionModel;

        [ObservableProperty] private string? _Title;

        public EmptyHostViewModel(INavigationService rootNavigationService, IVaultCollectionModel vaultCollectionModel)
        {
            ServiceProvider = Ioc.Default;
            _rootNavigationService = rootNavigationService;
            _vaultCollectionModel = vaultCollectionModel;
        }

        private async void VaultCollectionModel_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            await _rootNavigationService.TryNavigateAsync(() => new MainHostViewModel(_vaultCollectionModel), false);
        }

        [RelayCommand]
        private async Task AddNewVaultAsync()
        {
            await OverlayService.ShowAsync(new WizardOverlayViewModel(_vaultCollectionModel));
        }

        [RelayCommand]
        private async Task OpenSettingsAsync()
        {
            await OverlayService.ShowAsync(SettingsOverlayViewModel.Instance);
            await SettingsService.TrySaveAsync();
        }

        /// <inheritdoc/>
        public void OnAppearing()
        {
            _vaultCollectionModel.CollectionChanged += VaultCollectionModel_CollectionChanged;
        }

        /// <inheritdoc/>
        public void OnDisappearing()
        {
            _vaultCollectionModel.CollectionChanged -= VaultCollectionModel_CollectionChanged;
        }
    }
}
