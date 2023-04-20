using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;
using SecureFolderFS.Shared.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Dialogs
{
    public sealed partial class VaultWizardDialogViewModel : DialogViewModel, IDisposable
    {
        public INavigationService NavigationService { get; } = Ioc.Default.GetRequiredService<INavigationService>();

        [RelayCommand]
        private Task PrimaryButtonClickAsync(IEventDispatch? eventDispatch, CancellationToken cancellationToken)
        {
            if (NavigationService.CurrentTarget is BaseWizardPageViewModel wizardPageViewModel)
                return wizardPageViewModel.PrimaryButtonClickAsync(eventDispatch, cancellationToken);

            return Task.CompletedTask;
        }

        [RelayCommand]
        private Task SecondaryButtonClickAsync(IEventDispatch? eventDispatch, CancellationToken cancellationToken)
        {
            if (NavigationService.CurrentTarget is BaseWizardPageViewModel wizardPageViewModel)
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
