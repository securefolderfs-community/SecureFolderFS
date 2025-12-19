using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Shared;
using SecureFolderFS.UI.Enums;
using SecureFolderFS.Uno.Helpers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls.Introduction
{
    public sealed partial class WelcomeScreen : UserControl
    {
        public WelcomeScreen()
        {
            InitializeComponent();
        }

        private void BackgroundWebView_Loaded(object sender, RoutedEventArgs e)
        {
            var htmlString = UI.Constants.Introduction.BACKGROUND_WEBVIEW
                .Replace("c_bg", UnoThemeHelper.Instance.CurrentTheme switch
                {
                    ThemeType.Light => "vec3(0.88, 0.93, 0.98)",
                    _ => "vec3(0.00, 0.08, 0.15)"
                })
                .Replace("c_wave", UnoThemeHelper.Instance.CurrentTheme switch
                {
                    ThemeType.Light => "vec3(0.12, 0.48, 0.85)",
                    _ => "vec3(0.090, 0.569, 1.0)"
                });
            
            BackgroundWebView.NavigateToString(htmlString);
        }
    }
}
