using System.Windows.Input;

namespace SecureFolderFS.Maui.Views.Modals
{
    public partial class BaseModalPage : ContentPage
    {
        public BaseModalPage()
        {
            BindingContext = this;
            InitializeComponent();

            CloseButton.IsVisible = !string.IsNullOrEmpty(CloseText);
            PrimaryButton.IsVisible = !string.IsNullOrEmpty(PrimaryText);
        }

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
            BindableProperty.Create(nameof(ModalContent), typeof(ICommand), typeof(BaseModalPage), null, propertyChanged:
                (bindable, _, newValue) =>
                {
                    if (bindable is not BaseModalPage page)
                        return;

                    if (newValue is not ICommand value)
                        return;

                    page.PrimaryButton.Command = value;
                });

        public ICommand CloseCommand
        {
            get => (ICommand)GetValue(CloseCommandProperty);
            set => SetValue(CloseCommandProperty, value);
        }
        public static readonly BindableProperty CloseCommandProperty =
            BindableProperty.Create(nameof(ModalContent), typeof(ICommand), typeof(BaseModalPage), null, propertyChanged:
                (bindable, _, newValue) =>
                {
                    if (bindable is not BaseModalPage page)
                        return;

                    if (newValue is not ICommand value)
                        return;

                    page.CloseButton.Command = value;
                });

        public string? PrimaryText
        {
            get => (string?)GetValue(PrimaryTextProperty);
            set => SetValue(PrimaryTextProperty, value);
        }
        public static readonly BindableProperty PrimaryTextProperty =
            BindableProperty.Create(nameof(ModalContent), typeof(string), typeof(BaseModalPage), null, propertyChanged:
                (bindable, _, newValue) =>
                {
                    if (bindable is not BaseModalPage page)
                        return;

                    page.PrimaryButton.IsVisible = !string.IsNullOrEmpty((string?)newValue);
                    page.PrimaryButton.Text = (string?)newValue ?? string.Empty;
                });

        public string? CloseText
        {
            get => (string?)GetValue(CloseTextProperty);
            set => SetValue(CloseTextProperty, value);
        }
        public static readonly BindableProperty CloseTextProperty =
            BindableProperty.Create(nameof(ModalContent), typeof(string), typeof(BaseModalPage), null, propertyChanged:
                (bindable, _, newValue) =>
                {
                    if (bindable is not BaseModalPage page)
                        return;

                    page.CloseButton.IsVisible = !string.IsNullOrEmpty((string?)newValue);
                    page.CloseButton.Text = (string?)newValue ?? string.Empty;
                });

        public bool ContinueEnabled
        {
            get => (bool)GetValue(ContinueEnabledProperty);
            set => SetValue(ContinueEnabledProperty, value);
        }
        public static readonly BindableProperty ContinueEnabledProperty =
            BindableProperty.Create(nameof(ModalContent), typeof(bool), typeof(BaseModalPage), true, propertyChanged:
                (bindable, _, newValue) =>
                {
                    if (bindable is not BaseModalPage page)
                        return;

                    if (newValue is not bool value)
                        return;

                    page.PrimaryButton.IsEnabled = value;
                });

        public bool CloseEnabled
        {
            get => (bool)GetValue(CloseEnabledProperty);
            set => SetValue(CloseEnabledProperty, value);
        }
        public static readonly BindableProperty CloseEnabledProperty =
            BindableProperty.Create(nameof(ModalContent), typeof(bool), typeof(BaseModalPage), true, propertyChanged:
                (bindable, _, newValue) =>
                {
                    if (bindable is not BaseModalPage page)
                        return;

                    if (newValue is not bool value)
                        return;

                    page.PrimaryButton.IsEnabled = value;
                });
    }
}
