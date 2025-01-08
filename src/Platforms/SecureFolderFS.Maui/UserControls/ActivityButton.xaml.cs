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
            BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ActivityButton), null);

        public string? Text
        {
            get => (string?)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public static readonly BindableProperty TextProperty =
            BindableProperty.Create(nameof(Text), typeof(string), typeof(ActivityButton), null,
                propertyChanged: (bindable, oldValue, newValue) =>
                {
                    if (bindable is not ActivityButton activityButton)
                        return;

                    if (activityButton.IsProgressing)
                        return;

#if ANDROID
                    activityButton.AndroidButton.Text = activityButton.Text;
#elif IOS
                    activityButton.IOSButton.Text = activityButton.Text;
#endif
                });

        public bool IsProgressing
        {
            get => (bool)GetValue(IsProgressingProperty);
            set => SetValue(IsProgressingProperty, value);
        }
        public static readonly BindableProperty IsProgressingProperty =
            BindableProperty.Create(nameof(IsProgressing), typeof(bool), typeof(ActivityButton), false,
                propertyChanged: (bindable, oldValue, newValue) =>
                {
                    if (bindable is not ActivityButton activityButton)
                        return;

#if ANDROID
                    activityButton.AndroidButton.Text = activityButton.IsProgressing
                        ? string.Empty
                        : activityButton.Text;
#elif IOS
                    activityButton.IOSButton.Text = activityButton.IsProgressing
                        ? string.Empty
                        : activityButton.Text;
#endif
                });
    }
}
