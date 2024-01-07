using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard2;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    public sealed partial class WizardOverlayViewModel(IVaultCollectionModel vaultCollectionModel) : DialogViewModel
    {
        [ObservableProperty] private BaseWizardViewModel? _CurrentView;

        public event EventHandler<NavigationRequestedEventArgs>? NavigationRequested;

        public IVaultCollectionModel VaultCollectionModel { get; } = vaultCollectionModel;

        [RelayCommand]
        private async Task ContinuationAsync(IEventDispatch? eventDispatch, CancellationToken cancellationToken)
        {
            if (CurrentView is null)
                return;

            var result = await CurrentView.TryContinueAsync(cancellationToken);
            if (!result.Successful)
                eventDispatch?.PreventForwarding();
            else
            {
                NavigationRequested?.Invoke(this, new(result, CurrentView));
            }
        }

        [RelayCommand]
        private async Task CancellationAsync(IEventDispatch? eventDispatch, CancellationToken cancellationToken)
        {
            if (CurrentView is null)
                return;

            var result = await CurrentView.TryCancelAsync(cancellationToken);
            if (!result.Successful)
                eventDispatch?.PreventForwarding();
        }

        partial void OnCurrentViewChanging(BaseWizardViewModel? oldValue, BaseWizardViewModel? newValue)
        {
            oldValue?.OnNavigatingFrom();
        }
    }
}
