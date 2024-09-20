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
                return Shared.Helpers.Result.Failure(null);
            
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            _ = await Shell.Current.CurrentPage.ShowPopupAsync(this);
            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;

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
        
        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.CurrentViewModel)
                && ViewModel?.CurrentViewModel is RecoveryPreviewControlViewModel)
            {
                ThisPopup.Size = new(ThisPopup.Size.Width, 300d);
            }
        }

        private void RootGrid_Loaded(object? sender, EventArgs e)
        {
            var displayInfo = DeviceDisplay.MainDisplayInfo;
            var width = displayInfo.Width / displayInfo.Density;

            width -= 32; // Artificial margin on left and right
            ThisPopup.Size = new(width, 232d);
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

