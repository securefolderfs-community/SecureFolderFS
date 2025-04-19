using System.Windows.Input;

namespace SecureFolderFS.Maui.UserControls
{
    public partial class ActivityButton : ContentView
    {
        public ActivityButton()
        {
            InitializeComponent();
        }

        public ICommand? Command
        {
            get => (ICommand?)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }
        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ActivityButton));

        public FontAttributes FontAttributes
        {
            get => (FontAttributes)GetValue(FontAttributesProperty);
            set => SetValue(FontAttributesProperty, value);
        }
        public static readonly BindableProperty FontAttributesProperty =
            BindableProperty.Create(nameof(FontAttributes), typeof(FontAttributes), typeof(ActivityButton), defaultValue: FontAttributes.None);

        public Style? ButtonStyle
        {
            get => (Style?)GetValue(ButtonStyleProperty);
            set => SetValue(ButtonStyleProperty, value);
        }
        public static readonly BindableProperty ButtonStyleProperty =
            BindableProperty.Create(nameof(ButtonStyle), typeof(Style), typeof(ActivityButton));

        public string? Text
        {
            get => (string?)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public static readonly BindableProperty TextProperty =
            BindableProperty.Create(nameof(Text), typeof(string), typeof(ActivityButton), propertyChanged:
                static (bindable, oldValue, newValue) =>
                {
                    if (bindable is not ActivityButton activityButton)
                        return;

                    if (activityButton.IsProgressing)
                        return;

                    activityButton.ActionButton.Text = activityButton.Text ?? string.Empty;
                });

        public bool IsProgressing
        {
            get => (bool)GetValue(IsProgressingProperty);
            set => SetValue(IsProgressingProperty, value);
        }
        public static readonly BindableProperty IsProgressingProperty =
            BindableProperty.Create(nameof(IsProgressing), typeof(bool), typeof(ActivityButton), false, propertyChanged:
                static (bindable, oldValue, newValue) =>
                {
                    if (bindable is not ActivityButton activityButton)
                        return;

                    activityButton.ActionButton.Text = activityButton.IsProgressing
                        ? string.Empty
                        : activityButton.Text ?? string.Empty;
                });
    }
}
