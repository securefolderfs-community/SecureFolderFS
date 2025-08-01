using System.ComponentModel;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;

namespace SecureFolderFS.Maui.Views.Modals.Wizard
{
    public partial class PickerSourceWizardPage : BaseModalPage
    {
        public PickerSourceWizardViewModel WizardViewModel { get; }

        public WizardOverlayViewModel OverlayViewModel { get; }

        public PickerSourceWizardPage(PickerSourceWizardViewModel wizardViewModel, WizardOverlayViewModel overlayViewModel)
        {
            BrowseButtonContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Right, 164d);
            WizardViewModel = wizardViewModel;
            OverlayViewModel = overlayViewModel;
            BindingContext = this;

            InitializeComponent();
        }

        /// <inheritdoc/>
        protected override void OnAppearing()
        {
            OverlayViewModel.CurrentViewModel = WizardViewModel;
            WizardViewModel.PropertyChanged += WizardViewModelPropertyChanged;
            base.OnAppearing();
        }

        /// <inheritdoc/>
        protected override void OnDisappearing()
        {
            WizardViewModel.PropertyChanged -= WizardViewModelPropertyChanged;
            base.OnDisappearing();
        }

        private void LayoutBrowseButton()
        {
            var browseButton =
#if ANDROID
                AndroidBrowseButton;
#elif IOS
                IOSBrowseButton;
#else
                (Button?)null;
#endif

            if (browseButton is null)
                return;

            const int adjustment = 64;
            var availableWidth = browseButton.Width;
            var textWidth = InvisibleSelectedLocation.Width;
            var gap = availableWidth - textWidth - adjustment;

            BrowseButtonContentLayout = new(Button.ButtonContentLayout.ImagePosition.Right, gap);
        }

        private async void WizardViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(WizardViewModel.SelectedLocation))
                return;

            // Wait for the UI to update
            await Task.Delay(50);
            LayoutBrowseButton();
        }

        private void ButtonStack_SizeChanged(object? sender, EventArgs e)
        {
            LayoutBrowseButton();
        }

        public Button.ButtonContentLayout? BrowseButtonContentLayout
        {
            get => (Button.ButtonContentLayout?)GetValue(BrowseButtonContentLayoutProperty);
            set => SetValue(BrowseButtonContentLayoutProperty, value);
        }
        public static readonly BindableProperty BrowseButtonContentLayoutProperty =
            BindableProperty.Create(nameof(BrowseButtonContentLayout), typeof(Button.ButtonContentLayout), typeof(PickerSourceWizardPage), defaultValue: null);
    }
}
