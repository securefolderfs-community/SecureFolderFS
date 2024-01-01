using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls
{
    public sealed partial class TitleBarControl : UserControl
    {
        private IApplicationService ApplicationService { get; } = Ioc.Default.GetRequiredService<IApplicationService>();

        public TitleBarControl()
        {
            InitializeComponent();
        }

        private void VersionTitle_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBlock textBlock)
                return;

            textBlock.Text = $"BETA - v{ApplicationService.AppVersion}";
        }
    }
}
