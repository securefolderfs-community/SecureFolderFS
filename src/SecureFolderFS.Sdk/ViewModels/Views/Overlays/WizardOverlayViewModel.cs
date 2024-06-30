using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.EventArguments;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    public sealed partial class WizardOverlayViewModel: OverlayViewModel, INavigatable
    {
        [ObservableProperty] private BaseWizardViewModel? _CurrentViewModel;

        public IVaultCollectionModel VaultCollectionModel { get; }

        /// <inheritdoc />
        public event EventHandler<NavigationRequestedEventArgs>? NavigationRequested;

        public WizardOverlayViewModel(IVaultCollectionModel vaultCollectionModel)
        {
            VaultCollectionModel = vaultCollectionModel;
            PrimaryButtonText = "Continue".ToLocalized();
            SecondaryButtonText = "Cancel".ToLocalized();
        }

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

            eventDispatch?.PreventForwarding();
            var result = await CurrentViewModel.TryContinueAsync(cancellationToken);
            if (result.Successful)
                NavigationRequested?.Invoke(this, new WizardNavigationRequestedEventArgs(result, CurrentViewModel));
        }

        [RelayCommand]
        private async Task CancellationAsync(IEventDispatch? eventDispatch, CancellationToken cancellationToken)
        {
            if (CurrentViewModel is null)
                return;

            var result = await CurrentViewModel.TryCancelAsync(cancellationToken);
            if (!result.Successful)
                eventDispatch?.PreventForwarding();
            else
                NavigationRequested?.Invoke(this, new CloseNavigationRequestedEventArgs(CurrentViewModel));
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

                PrimaryButtonEnabled = newValue.CanContinue;
                SecondaryButtonEnabled = newValue.CanCancel;
                PrimaryButtonText = newValue.ContinueText;
                SecondaryButtonText = newValue.CancelText;
            }

            Title = newValue?.Title;
        }

        private void CurrentViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (CurrentViewModel is null)
                return;

            switch (e.PropertyName)
            {
                case nameof(BaseWizardViewModel.CanContinue):
                    PrimaryButtonEnabled = CurrentViewModel.CanContinue;
                    break;

                case nameof(BaseWizardViewModel.CanCancel):
                    SecondaryButtonEnabled = CurrentViewModel.CanCancel;
                    break;

                case nameof(BaseWizardViewModel.ContinueText):
                    PrimaryButtonText = CurrentViewModel.ContinueText;
                    break;

                case nameof(BaseWizardViewModel.CancelText):
                    SecondaryButtonText = CurrentViewModel.CancelText;
                    break;

                case nameof(BaseWizardViewModel.Title):
                    Title = CurrentViewModel.Title;
                    break;
            }
        }
    }
}
