using System.Windows.Input;
using Microsoft.Maui.Controls.Shapes;

namespace SecureFolderFS.Maui.Views.Modals
{
    public partial class BaseModalPage : ContentPage
    {
        public BaseModalPage()
        {
            BindingContext = this;
            InitializeComponent();
        }

        private void SheetPrimaryButton_SizeChanged(object? sender, EventArgs e)
        {
#if IOS
            var displayInfo = DeviceDisplay.MainDisplayInfo;
            SheetTitle.Margin = new(-(SheetPrimaryButton.Width / displayInfo.Density), 0d, 0d, 0d);
#endif
        }

        private static void UpdateButtonsVisibility(BindableObject bindable)
        {
#if ANDROID
            if (bindable is not BaseModalPage modalPage)
                return;

            var closeVisible = !string.IsNullOrEmpty(modalPage.CloseText);
            var primaryVisible = !string.IsNullOrEmpty(modalPage.PrimaryText);
            
            modalPage.ButtonsGrid.IsVisible = primaryVisible || closeVisible;
#else
            _ = bindable;
#endif
        }

        public bool IsImmersive
        {
            get => (bool)GetValue(IsImmersiveProperty);
            set => SetValue(IsImmersiveProperty, value);
        }
        public static readonly BindableProperty IsImmersiveProperty =
            BindableProperty.Create(nameof(IsImmersive), typeof(bool), typeof(BaseModalPage), false, propertyChanged:
                (bindable, _, newValue) =>
                {
                    if (bindable is not BaseModalPage modalPage)
                        return;

                    modalPage.ModalBorder.StrokeShape = !(bool)newValue
                        ? new RoundRectangle() { CornerRadius = new(24, 24, 0, 0) }
                        : new Rectangle();
                });

        public View? ModalContent
        {
            get => (View?)GetValue(ModalContentProperty);
            set => SetValue(ModalContentProperty, value);
        }
        public static readonly BindableProperty ModalContentProperty =
            BindableProperty.Create(nameof(ModalContent), typeof(View), typeof(BaseModalPage), null);

        public ICommand? PrimaryCommand
        {
            get => (ICommand?)GetValue(PrimaryCommandProperty);
            set => SetValue(PrimaryCommandProperty, value);
        }
        public static readonly BindableProperty PrimaryCommandProperty =
            BindableProperty.Create(nameof(PrimaryCommand), typeof(ICommand), typeof(BaseModalPage), null);

        public ICommand CloseCommand
        {
            get => (ICommand)GetValue(CloseCommandProperty);
            set => SetValue(CloseCommandProperty, value);
        }
        public static readonly BindableProperty CloseCommandProperty =
            BindableProperty.Create(nameof(CloseCommand), typeof(ICommand), typeof(BaseModalPage), null);

        public string? PrimaryText
        {
            get => (string?)GetValue(PrimaryTextProperty);
            set => SetValue(PrimaryTextProperty, value);
        }
        public static readonly BindableProperty PrimaryTextProperty =
            BindableProperty.Create(nameof(PrimaryText), typeof(string), typeof(BaseModalPage), null, propertyChanged:
                (bindable, _, _) => UpdateButtonsVisibility(bindable));

        public string? CloseText
        {
            get => (string?)GetValue(CloseTextProperty);
            set => SetValue(CloseTextProperty, value);
        }
        public static readonly BindableProperty CloseTextProperty =
            BindableProperty.Create(nameof(CloseText), typeof(string), typeof(BaseModalPage), null, propertyChanged:
                (bindable, _, _) => UpdateButtonsVisibility(bindable));

        public bool PrimaryEnabled
        {
            get => (bool)GetValue(PrimaryEnabledProperty);
            set => SetValue(PrimaryEnabledProperty, value);
        }
        public static readonly BindableProperty PrimaryEnabledProperty =
            BindableProperty.Create(nameof(PrimaryEnabled), typeof(bool), typeof(BaseModalPage), true);

        public bool CloseEnabled
        {
            get => (bool)GetValue(CloseEnabledProperty);
            set => SetValue(CloseEnabledProperty, value);
        }
        public static readonly BindableProperty CloseEnabledProperty =
            BindableProperty.Create(nameof(CloseEnabled), typeof(bool), typeof(BaseModalPage), true);
    }
}
