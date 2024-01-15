using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Inject<INavigationService>(Visibility = "public")]
    public sealed partial class VaultWizardDialogViewModel : DialogViewModel, IDisposable
    {
        public IVaultCollectionModel VaultCollectionModel { get; }

        public VaultWizardDialogViewModel(IVaultCollectionModel vaultCollectionModel)
        {
            ServiceProvider = Ioc.Default;
            VaultCollectionModel = vaultCollectionModel;
        }

        [RelayCommand]
        private Task PrimaryButtonClickAsync(IEventDispatch? eventDispatch, CancellationToken cancellationToken)
        {
            if (NavigationService.CurrentView is BaseWizardPageViewModel wizardPageViewModel)
                return wizardPageViewModel.PrimaryButtonClickAsync(eventDispatch, cancellationToken);

            return Task.CompletedTask;
        }

        [RelayCommand]
        private Task SecondaryButtonClickAsync(IEventDispatch? eventDispatch, CancellationToken cancellationToken)
        {
            if (NavigationService.CurrentView is BaseWizardPageViewModel wizardPageViewModel)
                return wizardPageViewModel.SecondaryButtonClickAsync(eventDispatch, cancellationToken);

            return Task.CompletedTask;
        }

        [RelayCommand]
        private Task GoBackAsync()
        {
            return NavigationService.GoBackAsync();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            NavigationService.Dispose();
        }
    }
}
