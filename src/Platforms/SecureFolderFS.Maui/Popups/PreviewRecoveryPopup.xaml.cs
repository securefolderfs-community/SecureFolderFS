using CommunityToolkit.Maui.Views;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.Utils;

namespace SecureFolderFS.Maui.Popups
{
    public partial class PreviewRecoveryPopup : Popup, IOverlayControl
    {
        public PreviewRecoveryPopup()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public async Task<IResult> ShowAsync()
        {
            if (ViewModel is null)
                return Shared.Helpers.Result.Failure(null);
            
            var page = Shell.Current.CurrentPage;
            var result =  await page.ShowPopupAsync(this);

            return Shared.Helpers.Result.Success;
        }

        /// <inheritdoc/>
        public void SetView(IViewable viewable)
        {
            ViewModel = (PreviewRecoveryOverlayViewModel)viewable;
        }
        
        /// <inheritdoc/>
        public Task HideAsync()
        {
            return Task.CompletedTask;
        }
        
        public PreviewRecoveryOverlayViewModel? ViewModel
        {
            get => (PreviewRecoveryOverlayViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create(nameof(ViewModel), typeof(PreviewRecoveryOverlayViewModel), typeof(PreviewRecoveryPopup), null);
    }
}

