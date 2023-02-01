using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SecureFolderFS.Sdk.ViewModels.AppHost;

namespace SecureFolderFS.AvaloniaUI.UserControls.InterfaceHost
{
    public sealed partial class NoVaultsAppHostControl : UserControl
    {
        public NoVaultsAppHostControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
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