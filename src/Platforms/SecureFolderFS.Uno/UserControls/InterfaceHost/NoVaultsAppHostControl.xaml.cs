using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Host;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls.InterfaceHost
{
    public sealed partial class NoVaultsAppHostControl : UserControl
    {
        public NoVaultsAppHostControl()
        {
            InitializeComponent();
        }
        
        private async void IntroButton_Click(object sender, RoutedEventArgs e)
        {
            var overlayService = DI.Service<IOverlayService>();
            var overlay = new IntroductionOverlayViewModel(10);

            await overlayService.ShowAsync(overlay);
        }

        public EmptyHostViewModel ViewModel
        {
            get => (EmptyHostViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(EmptyHostViewModel), typeof(NoVaultsAppHostControl), new PropertyMetadata(defaultValue: null));
    }
}
