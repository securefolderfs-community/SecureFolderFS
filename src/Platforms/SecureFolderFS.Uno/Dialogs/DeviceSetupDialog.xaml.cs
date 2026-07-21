using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.Utils;
using SecureFolderFS.Uno.Extensions;

namespace SecureFolderFS.Uno.Dialogs
{
    public sealed partial class DeviceSetupDialog : ContentDialog, IOverlayControl
    {
        public DeviceSetupOverlayViewModel? ViewModel
        {
            get => DataContext.TryCast<DeviceSetupOverlayViewModel>();
            set => DataContext = value;
        }

        public DeviceSetupDialog()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public new async Task<IResult> ShowAsync()
        {
            var result = await base.ShowAsync();

            if (result == ContentDialogResult.Primary && ViewModel is not null)
                ViewModel.Passphrase = PassphraseBox.Password;

            return result.ParseOverlayOption();
        }

        private void ForgotPassphraseLink_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(ForgotPassphraseLink);
        }

        private void ConfirmReset_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel is null)
                return;

            ResetFlyout.Hide();
            ViewModel.ResetRequested = true;
            Hide();
        }

        /// <inheritdoc/>
        public void SetView(IViewable viewable) => ViewModel = (DeviceSetupOverlayViewModel)viewable;

        /// <inheritdoc/>
        public Task HideAsync()
        {
            Hide();
            return Task.CompletedTask;
        }
    }
}
