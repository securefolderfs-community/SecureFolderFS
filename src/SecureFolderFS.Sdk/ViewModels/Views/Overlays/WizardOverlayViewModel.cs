using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard2;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    public sealed partial class WizardOverlayViewModel(IVaultCollectionModel vaultCollectionModel) : DialogViewModel
    {
        [ObservableProperty] private BaseWizardViewModel? _CurrentViewModel;

        public event EventHandler<NavigationRequestedEventArgs>? NavigationRequested;

        public IVaultCollectionModel VaultCollectionModel { get; } = vaultCollectionModel;

        /// <inheritdoc />
        public override void OnDisappearing()
        {
            if (CurrentViewModel is not null)
                CurrentViewModel.PropertyChanged -= CurrentViewModel_PropertyChanged;
        }

        [RelayCommand]
        private async Task ContinuationAsync(IEventDispatch? eventDispatch, CancellationToken cancellationToken)
        {
            if (CurrentViewModel is null)
                return;

            var result = await CurrentViewModel.TryContinueAsync(cancellationToken);
            if (!result.Successful)
                eventDispatch?.PreventForwarding();
            else
            {
                NavigationRequested?.Invoke(this, new(result, CurrentViewModel));
            }
        }

        [RelayCommand]
        private async Task CancellationAsync(IEventDispatch? eventDispatch, CancellationToken cancellationToken)
        {
            if (CurrentViewModel is null)
                return;

            var result = await CurrentViewModel.TryCancelAsync(cancellationToken);
            if (!result.Successful)
                eventDispatch?.PreventForwarding();
        }

        partial void OnCurrentViewModelChanging(BaseWizardViewModel? oldValue, BaseWizardViewModel? newValue)
        {
            if (oldValue is not null)
            {
                oldValue.PropertyChanged -= CurrentViewModel_PropertyChanged;
                oldValue.OnDisappearing();
            }

            if (newValue is not null)
            {
                newValue.PropertyChanged += CurrentViewModel_PropertyChanged;
                newValue.OnAppearing();
            }

            Title = newValue?.Title;
        }

        private void CurrentViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IViewable.Title))
                Title = CurrentViewModel?.Title;
        }
    }
}
