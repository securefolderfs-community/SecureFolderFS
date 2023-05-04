using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Utils;
using SecureFolderFS.UI.AppModels;
using System;
using System.Windows.Input;

namespace SecureFolderFS.AvaloniaUI.UserControls
{
    public sealed partial class PasswordControl : UserControl, IEquatable<PasswordControl>
    {
        public PasswordControl()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <inheritdoc/>
        public bool Equals(PasswordControl? other)
        {
            return PasswordInput.Text.Equals(other?.PasswordInput.Text);
        }

        public IPassword? GetPassword()
        {
            if (PasswordInput.Text.IsEmpty())
                return null;

            return new VaultPassword(PasswordInput.Text);
        }

        private void PasswordInput_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                PasswordSubmittedCommand?.Execute(GetPassword());
        }

        private void PasswordInput_PasswordChanged(object? sender, TextChangedEventArgs e)
        {
            PasswordChanged?.Invoke(sender, e);
        }

        // TODO Use RoutedEventHandler
        public event EventHandler<RoutedEventArgs>? PasswordChanged;

        public ICommand? PasswordSubmittedCommand
        {
            get => GetValue(PasswordSubmittedCommandProperty);
            set => SetValue(PasswordSubmittedCommandProperty, value);
        }
        public static readonly StyledProperty<ICommand?> PasswordSubmittedCommandProperty =
            AvaloniaProperty.Register<PasswordControl, ICommand?>(nameof(PasswordSubmittedCommand));

        public string? Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly StyledProperty<string?> TitleProperty =
            AvaloniaProperty.Register<PasswordControl, string?>(nameof(Title), "Enter password");

        public string? Placeholder
        {
            get => GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }
        public static readonly StyledProperty<string?> PlaceholderProperty =
            AvaloniaProperty.Register<PasswordControl, string?>(nameof(Placeholder), "Password");

        public bool ShowInvalidPasswordMessage
        {
            get => GetValue(ShowInvalidPasswordMessageProperty);
            set => SetValue(ShowInvalidPasswordMessageProperty, value);
        }
        public static readonly StyledProperty<bool> ShowInvalidPasswordMessageProperty =
            AvaloniaProperty.Register<PasswordControl, bool>(nameof(ShowInvalidPasswordMessage));
    }
}
