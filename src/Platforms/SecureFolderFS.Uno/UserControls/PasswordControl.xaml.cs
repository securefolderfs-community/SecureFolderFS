using System;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls
{
    public sealed partial class PasswordControl : UserControl, IEquatable<PasswordControl>
    {
        public event RoutedEventHandler? PasswordChanged;
        public event RoutedEventHandler? PasswordSubmitted;

        public PasswordControl()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public bool Equals(PasswordControl? other)
        {
            return PasswordInput.Password.Equals(other?.PasswordInput.Password);
        }

        public IPassword? GetPassword()
        {
            if (PasswordInput.Password.IsEmpty())
                return null;

            return new DisposablePassword(PasswordInput.Password);
        }

        private void PasswordInput_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (IsKeyboardEventTrackingEnabled && e.Key == VirtualKey.Enter)
            {
                PasswordSubmittedCommand?.Execute(GetPassword());
                PasswordSubmitted?.Invoke(this, new RoutedEventArgs());
            }
        }

        private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordChanged?.Invoke(sender, e);
        }

        public ICommand? PasswordSubmittedCommand
        {
            get => (ICommand?)GetValue(PasswordSubmittedCommandProperty);
            set => SetValue(PasswordSubmittedCommandProperty, value);
        }
        public static readonly DependencyProperty PasswordSubmittedCommandProperty =
            DependencyProperty.Register(nameof(PasswordSubmittedCommand), typeof(ICommand), typeof(PasswordControl), new PropertyMetadata(defaultValue: null));

        public string UnsecurePassword
        {
            get => (string)GetValue(UnsecurePasswordProperty);
            set => SetValue(UnsecurePasswordProperty, value);
        }
        public static readonly DependencyProperty UnsecurePasswordProperty =
            DependencyProperty.Register(nameof(UnsecurePassword), typeof(string), typeof(PasswordControl), new PropertyMetadata(defaultValue: null));

        public string? Title
        {
            get => (string?)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(PasswordControl), new PropertyMetadata(defaultValue: "EnterPassword".ToLocalized()));

        public string? Placeholder
        {
            get => (string?)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(PasswordControl), new PropertyMetadata(defaultValue: "Password".ToLocalized()));

        public bool ShowInvalidPasswordMessage
        {
            get => (bool)GetValue(ShowInvalidPasswordMessageProperty);
            set => SetValue(ShowInvalidPasswordMessageProperty, value);
        }
        public static readonly DependencyProperty ShowInvalidPasswordMessageProperty =
            DependencyProperty.Register(nameof(ShowInvalidPasswordMessage), typeof(bool), typeof(PasswordControl), new PropertyMetadata(defaultValue: false));

        public bool IsKeyboardEventTrackingEnabled
        {
            get => (bool)GetValue(IsKeyboardEventTrackingEnabledProperty);
            set => SetValue(IsKeyboardEventTrackingEnabledProperty, value);
        }
        public static readonly DependencyProperty IsKeyboardEventTrackingEnabledProperty =
            DependencyProperty.Register(nameof(IsKeyboardEventTrackingEnabled), typeof(bool), typeof(PasswordControl), new PropertyMetadata(true));
    }
}
