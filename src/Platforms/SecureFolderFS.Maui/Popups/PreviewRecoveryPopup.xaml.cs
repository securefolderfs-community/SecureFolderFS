using System.ComponentModel;
using CommunityToolkit.Maui.Views;
using SecureFolderFS.Sdk.ViewModels.Controls;
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
            
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            _ = await Shell.Current.CurrentPage.ShowPopupAsync(this);
            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;

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
        
        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.CurrentViewModel)
                && ViewModel?.CurrentViewModel is RecoveryPreviewControlViewModel)
            {
#if IOS
                var height = 300d;
#else
                var height = 376d;
#endif
                
                var displayInfo = DeviceDisplay.MainDisplayInfo;
                var width = (displayInfo.Width / displayInfo.Density) - 38; // Account for artificial margin
                ThisPopup.Size = new(width, height);
            }
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

