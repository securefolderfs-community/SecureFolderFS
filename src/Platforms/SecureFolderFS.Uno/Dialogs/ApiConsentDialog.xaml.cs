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
    public sealed partial class ApiConsentDialog : ContentDialog, IOverlayControl
    {
        public ApiConsentOverlayViewModel? ViewModel
        {
            get => DataContext.TryCast<ApiConsentOverlayViewModel>();
            set => DataContext = value;
        }

        /// <summary>
        /// Gets the severity used for the identity notice.
        /// </summary>
        public InfoBarSeverity IdentitySeverity => ViewModel?.IsIdentityVerified == true
            ? InfoBarSeverity.Success
            : InfoBarSeverity.Warning;

        public ApiConsentDialog()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public new async Task<IResult> ShowAsync() => (await base.ShowAsync()).ParseOverlayOption();

        /// <inheritdoc/>
        public void SetView(IViewable viewable) => ViewModel = (ApiConsentOverlayViewModel)viewable;

        /// <inheritdoc/>
        public Task HideAsync()
        {
            Hide();
            return Task.CompletedTask;
        }
    }
}
