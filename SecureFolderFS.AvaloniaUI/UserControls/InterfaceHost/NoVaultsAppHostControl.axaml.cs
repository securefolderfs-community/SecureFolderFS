using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using SecureFolderFS.Sdk.ViewModels.AppHost;

namespace SecureFolderFS.AvaloniaUI.UserControls.InterfaceHost
{
    public sealed partial class NoVaultsAppHostControl : UserControl
    {
        public NoVaultsAppHostControl()
        {
            InitializeComponent();

            SettingsButton.AddHandler(PointerPressedEvent, SettingsButton_OnPointerPressed, handledEventsToo: true);
            SettingsButton.AddHandler(PointerReleasedEvent, SettingsButton_OnPointerReleased, handledEventsToo: true);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void SettingsButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            SpinSettingsIconPointerPressedStoryboard.RunAnimationsAsync();
        }

        private void SettingsButton_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            SpinSettingsIconPointerReleasedStoryboard.RunAnimationsAsync();
        }

        public NoVaultsAppHostViewModel ViewModel
        {
            get => GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public static readonly StyledProperty<NoVaultsAppHostViewModel> ViewModelProperty
            = AvaloniaProperty.Register<NoVaultsAppHostControl, NoVaultsAppHostViewModel>(nameof(ViewModel));
    }
}