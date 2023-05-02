using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using SecureFolderFS.Sdk.ViewModels.Views.Host;

namespace SecureFolderFS.AvaloniaUI.UserControls.InterfaceHost
{
    public sealed partial class NoVaultsAppHostControl : UserControl
    {
        public NoVaultsAppHostControl()
        {
            AvaloniaXamlLoader.Load(this);

            SettingsButton.AddHandler(PointerPressedEvent, SettingsButton_PointerPressed, handledEventsToo: true);
            SettingsButton.AddHandler(PointerReleasedEvent, SettingsButton_PointerReleased, handledEventsToo: true);
        }

        private void SettingsButton_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            SpinSettingsIconPointerPressedStoryboard.BeginAsync();
        }

        private void SettingsButton_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            SpinSettingsIconPointerReleasedStoryboard.BeginAsync();
        }

        public EmptyHostViewModel? ViewModel
        {
            get => GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly StyledProperty<EmptyHostViewModel?> ViewModelProperty
            = AvaloniaProperty.Register<NoVaultsAppHostControl, EmptyHostViewModel?>(nameof(ViewModel));
    }
}