using System.Windows.Input;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using SecureFolderFS.Sdk.Extensions;

namespace SecureFolderFS.Maui.UserControls
{
    public partial class PasswordControl : ContentView
    {
        public event EventHandler? PasswordSubmitted;

        public PasswordControl()
        {
            InitializeComponent();
            RootGrid.BindingContext = this;
        }

        private void PasswordEntry_Completed(object? sender, EventArgs e)
        {
            PasswordSubmittedCommand?.Execute(null);
            PasswordSubmitted?.Invoke(this, EventArgs.Empty);
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
            BindableProperty.Create(nameof(UnsecurePassword), typeof(string), typeof(PasswordControl), defaultBindingMode: BindingMode.TwoWay);

        public bool ShowInvalidPasswordMessage
        {
            get => (bool)GetValue(ShowInvalidPasswordMessageProperty);
            set => SetValue(ShowInvalidPasswordMessageProperty, value);
        }
        public static readonly BindableProperty ShowInvalidPasswordMessageProperty =
            BindableProperty.Create(nameof(ShowInvalidPasswordMessage), typeof(bool), typeof(PasswordControl), defaultValue: false, defaultBindingMode: BindingMode.TwoWay,
                propertyChanged: static async (bindable, _, newValue) =>
                {
                    if (newValue is not true)
                        return;

                    if (bindable is not PasswordControl passwordControl)
                        return;

                    var toast = Toast.Make("InvalidPassword".ToLocalized(), ToastDuration.Short);
                    await toast.Show();

                    passwordControl.ShowInvalidPasswordMessage = false;
                });

        public ICommand? PasswordSubmittedCommand
        {
            get => (ICommand?)GetValue(PasswordSubmittedCommandProperty);
            set => SetValue(PasswordSubmittedCommandProperty, value);
        }
        public static readonly BindableProperty PasswordSubmittedCommandProperty =
            BindableProperty.Create(nameof(PasswordSubmittedCommand), typeof(ICommand), typeof(PasswordControl));
    }
}
