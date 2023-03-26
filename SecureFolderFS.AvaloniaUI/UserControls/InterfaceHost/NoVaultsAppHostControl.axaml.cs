using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SecureFolderFS.Sdk.ViewModels.Views.Host;

namespace SecureFolderFS.AvaloniaUI.UserControls.InterfaceHost
{
    public sealed partial class NoVaultsAppHostControl : UserControl
    {
        public NoVaultsAppHostControl()
        {
            AvaloniaXamlLoader.Load(this);
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