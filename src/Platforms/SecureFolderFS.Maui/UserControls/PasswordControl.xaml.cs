using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using SecureFolderFS.Sdk.Extensions;

namespace SecureFolderFS.Maui.UserControls
{
    public partial class PasswordControl : ContentView
    {
        public PasswordControl()
        {
            InitializeComponent();
            RootGrid.BindingContext = this;
        }

        public string? Title
        {
            get => (string?)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(PasswordControl), defaultValue: "EnterPassword".ToLocalized());

        public string? Placeholder
        {
            get => (string?)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }
        public static readonly BindableProperty PlaceholderProperty =
            BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(PasswordControl), defaultValue: "Password".ToLocalized());

        public string? UnsecurePassword
        {
            get => (string?)GetValue(UnsecurePasswordProperty);
            set => SetValue(UnsecurePasswordProperty, value);
        }
        public static readonly BindableProperty UnsecurePasswordProperty =
            BindableProperty.Create(nameof(UnsecurePassword), typeof(string), typeof(PasswordControl), defaultValue: null, defaultBindingMode: BindingMode.TwoWay);

        public bool ShowInvalidPasswordMessage
        {
            get => (bool)GetValue(ShowInvalidPasswordMessageProperty);
            set => SetValue(ShowInvalidPasswordMessageProperty, value);
        }
        public static readonly BindableProperty ShowInvalidPasswordMessageProperty =
            BindableProperty.Create(nameof(ShowInvalidPasswordMessage), typeof(bool), typeof(PasswordControl), defaultValue: false, defaultBindingMode: BindingMode.TwoWay,
                propertyChanged: async (bindable, value, newValue) =>
                {
                    if (newValue is not (bool and true))
                        return;
                    
                    if (bindable is not PasswordControl passwordControl)
                        return;

                    var toast = Toast.Make("InvalidPassword".ToLocalized(), ToastDuration.Short);
                    await toast.Show();

                    passwordControl.ShowInvalidPasswordMessage = false;
                });
    }
}
