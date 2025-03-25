using CommunityToolkit.Maui.Views;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.Utils;

namespace SecureFolderFS.Maui.Popups
{
    public partial class PropertiesPopup : Popup, IOverlayControl
    {
        public PropertiesPopup()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public async Task<IResult> ShowAsync()
        {
            if (ViewModel is null)
                return Shared.Models.Result.Failure(null);

            _ = await Shell.Current.CurrentPage.ShowPopupAsync(this);
            return Shared.Models.Result.Success;
        }

        /// <inheritdoc/>
        public void SetView(IViewable viewable)
        {
            ViewModel = (PropertiesOverlayViewModel)viewable;
        }

        /// <inheritdoc/>
        public Task HideAsync()
        {
            return CloseAsync();
        }

        public PropertiesOverlayViewModel? ViewModel
        {
            get => (PropertiesOverlayViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create(nameof(ViewModel), typeof(PropertiesOverlayViewModel), typeof(PropertiesPopup), null);
    }
}

