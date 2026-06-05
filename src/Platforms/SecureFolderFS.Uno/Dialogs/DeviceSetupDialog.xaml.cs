using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
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
