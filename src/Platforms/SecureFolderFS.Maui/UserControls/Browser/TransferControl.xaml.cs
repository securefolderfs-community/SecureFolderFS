using System.Windows.Input;

namespace SecureFolderFS.Maui.UserControls.Browser
{
    public partial class TransferControl : ContentView
    {
        public TransferControl()
        {
            InitializeComponent();
        }

        public bool IsShown
        {
            get => (bool)GetValue(IsShownProperty);
            set => SetValue(IsShownProperty, value);
        }
        public static readonly BindableProperty IsShownProperty =
            BindableProperty.Create(nameof(IsShown), typeof(bool), typeof(TransferControl), false, propertyChanged:
                static async (bindable, _, newValue) =>
                {
                    if (newValue is not bool bValue)
                        return;

                    if (bindable is not TransferControl transferControl)
                        return;

                    if (bValue)
                    {
                        transferControl.RootPanel.TranslationY = 200d;
                        transferControl.RootPanel.IsVisible = true;
                        await transferControl.RootPanel.TranslateTo(0, 0, 350U, Easing.CubicInOut);
                    }
                    else
                    {
                        transferControl.RootPanel.TranslationY = 0d;
                        await transferControl.RootPanel.TranslateTo(0, 200, 350U, Easing.CubicInOut);
                        transferControl.RootPanel.IsVisible = false;
                    }
                });

        public bool CanCancel
        {
            get => (bool)GetValue(CanCancelProperty);
            set => SetValue(CanCancelProperty, value);
        }
        public static readonly BindableProperty CanCancelProperty =
            BindableProperty.Create(nameof(CanCancel), typeof(bool), typeof(TransferControl), true);

        public bool IsProgressing
        {
            get => (bool)GetValue(IsProgressingProperty);
            set => SetValue(IsProgressingProperty, value);
        }
        public static readonly BindableProperty IsProgressingProperty =
            BindableProperty.Create(nameof(IsProgressing), typeof(bool), typeof(TransferControl), false);

        public string? Title
        {
            get => (string?)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(TransferControl));

        public string? PrimaryButtonText
        {
            get => (string?)GetValue(PrimaryButtonTextProperty);
            set => SetValue(PrimaryButtonTextProperty, value);
        }
        public static readonly BindableProperty PrimaryButtonTextProperty =
            BindableProperty.Create(nameof(PrimaryButtonText), typeof(string), typeof(TransferControl));

        public ICommand? CancelCommand
        {
            get => (ICommand?)GetValue(CancelCommandProperty);
            set => SetValue(CancelCommandProperty, value);
        }
        public static readonly BindableProperty CancelCommandProperty =
            BindableProperty.Create(nameof(CancelCommand), typeof(ICommand), typeof(TransferControl));

        public ICommand? PrimaryCommand
        {
            get => (ICommand?)GetValue(PrimaryCommandProperty);
            set => SetValue(PrimaryCommandProperty, value);
        }
        public static readonly BindableProperty PrimaryCommandProperty =
            BindableProperty.Create(nameof(PrimaryCommand), typeof(ICommand), typeof(TransferControl));
    }
}
