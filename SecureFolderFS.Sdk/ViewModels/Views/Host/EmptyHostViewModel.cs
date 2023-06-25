using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Host
{
    public sealed partial class EmptyHostViewModel : BasePageViewModel
    {
        private readonly INavigationService _hostNavigationService;
        private readonly IVaultCollectionModel _vaultCollectionModel;

        private IDialogService DialogService { get; } = Ioc.Default.GetRequiredService<IDialogService>();

        private ISettingsService SettingsService { get; } = Ioc.Default.GetRequiredService<ISettingsService>();

        public EmptyHostViewModel(INavigationService hostNavigationService, IVaultCollectionModel vaultCollectionModel)
        {
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
            await SettingsService.SaveAsync();
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
