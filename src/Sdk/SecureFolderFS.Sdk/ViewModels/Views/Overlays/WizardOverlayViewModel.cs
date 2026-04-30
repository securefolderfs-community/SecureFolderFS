using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.EventArguments;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    public sealed partial class WizardOverlayViewModel : OverlayViewModel, IStagingView, INavigatable, IDisposable
    {
        [ObservableProperty] private IViewDesignation? _CurrentViewModel;

        public IVaultCollectionModel VaultCollectionModel { get; }

        /// <inheritdoc />
        public event EventHandler<NavigationRequestedEventArgs>? NavigationRequested;

        public WizardOverlayViewModel(IVaultCollectionModel vaultCollectionModel)
        {
            VaultCollectionModel = vaultCollectionModel;
            PrimaryText = "Continue".ToLocalized();
            SecondaryText = "Cancel".ToLocalized();
        }

        /// <inheritdoc />
        public override void OnDisappearing()
        {
            if (CurrentViewModel is not null)
                CurrentViewModel.PropertyChanged -= CurrentViewModel_PropertyChanged;
        }

        /// <inheritdoc />
        public async Task<IResult> TryContinueAsync(CancellationToken cancellationToken)
        {
            if (CurrentViewModel is not IStagingView stagingView)
                return Result.Failure(null);

            var result = await stagingView.TryContinueAsync(cancellationToken);
            if (result.Successful)
                NavigationRequested?.Invoke(this, new WizardNavigationRequestedEventArgs(result, CurrentViewModel));

            return result;
        }

        /// <inheritdoc />
        public async Task<IResult> TryCancelAsync(CancellationToken cancellationToken)
        {
            if (CurrentViewModel is not IStagingView stagingView)
                return Result.Failure(null);

            var result = await stagingView.TryCancelAsync(cancellationToken);
            if (result.Successful)
                NavigationRequested?.Invoke(this, new DismissNavigationRequestedEventArgs(CurrentViewModel));

            return result;
        }

        [RelayCommand]
        private async Task ContinuationAsync(IEventDispatch? eventDispatch, CancellationToken cancellationToken)
        {
            eventDispatch?.PreventForwarding();
            await TryContinueAsync(cancellationToken);
        }

        [RelayCommand]
        private async Task CancellationAsync(IEventDispatch? eventDispatch, CancellationToken cancellationToken)
        {
            var result = await TryCancelAsync(cancellationToken);
            if (!result.Successful)
                eventDispatch?.PreventForwarding();
        }

        partial void OnCurrentViewModelChanging(IViewDesignation? oldValue, IViewDesignation? newValue)
        {
            oldValue?.PropertyChanged -= CurrentViewModel_PropertyChanged;
            oldValue?.OnDisappearing();

            newValue?.PropertyChanged += CurrentViewModel_PropertyChanged;
            newValue?.OnAppearing();
            Title = newValue?.Title;

            if (newValue is OverlayViewModel overlayViewModel)
            {
                CanContinue = overlayViewModel.CanContinue;
                CanCancel = overlayViewModel.CanCancel;
                PrimaryText = overlayViewModel.PrimaryText;
                SecondaryText = overlayViewModel.SecondaryText;
            }
        }

        private void CurrentViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (CurrentViewModel is not OverlayViewModel currentViewModel)
                return;

            switch (e.PropertyName)
            {
                case nameof(CanContinue):
                    CanContinue = currentViewModel.CanContinue;
                    break;

                case nameof(CanCancel):
                    CanCancel = currentViewModel.CanCancel;
                    break;

                case nameof(PrimaryText):
                    PrimaryText = currentViewModel.PrimaryText;
                    break;

                case nameof(SecondaryText):
                    SecondaryText = currentViewModel.SecondaryText;
                    break;

                case nameof(Title):
                    Title = currentViewModel.Title;
                    break;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (CurrentViewModel is not null)
                CurrentViewModel.PropertyChanged -= CurrentViewModel_PropertyChanged;
        }
    }
}
