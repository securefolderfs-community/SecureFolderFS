using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
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
                return Shared.Models.Result.Failure(null);

            _ = await Shell.Current.CurrentPage.ShowPopupAsync(this, new PopupOptions()
            {
                PageOverlayColor = Colors.Transparent
            });
            return Shared.Models.Result.Success;
        }

        /// <inheritdoc/>
        public void SetView(IViewable viewable)
        {
            ViewModel = (PreviewRecoveryOverlayViewModel)viewable;
        }

        /// <inheritdoc/>
        public Task HideAsync()
        {
            return CloseAsync();
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

