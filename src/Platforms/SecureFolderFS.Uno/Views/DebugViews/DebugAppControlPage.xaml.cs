using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.ViewModels.Views.Host;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Uno.UserControls;
using SecureFolderFS.Uno.UserControls.DebugControls;
using SecureFolderFS.Uno.UserControls.InterfaceRoot;
using Windows.UI.Composition;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SecureFolderFS.Uno.Views.DebugViews
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DebugAppControlPage : Page
    {
        private MainWindowRootControl? _rootControl;
        private TitleBarControl? _titleBar;
        private TextBlock? _titleBarPart1;
        private TextBlock? _titleBarPart2;

        public DebugAppControlPage()
        {
            InitializeComponent();
        }

        private async void DebugAppControlPage_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(1000);

            _rootControl = App.Instance?.MainWindow?.Content as MainWindowRootControl;
#if WINDOWS
            _titleBar = _rootControl?.CustomTitleBar;
#endif
            _titleBarPart1 = _titleBar?.TitlePanel?.Children?[0] as TextBlock;
            _titleBarPart2 = _titleBar?.TitlePanel?.Children?[1] as TextBlock;

            // App Info
            Dbg_AppInfo_AreEffectsFast.Text = CompositionCapabilities.GetForCurrentView().AreEffectsFast().ToString();

            // App Title
            Dbg_AppTitle_Part1.Text = _titleBarPart1?.Text ??
#if !__IOS__
                                      App.Instance?.MainWindow?.Title;
#else
                                      null;
#endif
            Dbg_AppTitle_Part2.Text = _titleBarPart2?.Text;
        }

        private void AppTitlePart1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_titleBarPart1 is not null)
                _titleBarPart1.Text = Dbg_AppTitle_Part1.Text;
            else if (App.Instance?.MainWindow is { } mainWindow)
            {
#if !__IOS__
                mainWindow.Title = Dbg_AppTitle_Part1.Text;
#endif
            }
        }

        private void AppTitlePart2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_titleBarPart2 is not null)
                _titleBarPart2.Text = Dbg_AppTitle_Part2.Text;
        }

        private void RefreshPrimaryView_Click(object sender, RoutedEventArgs e)
        {
            if (_rootControl?.RootNavigationService.CurrentView is not MainHostViewModel mainHost)
                return;

            Dbg_PrimaryView_Presenter.Content = mainHost.NavigationService.CurrentView switch
            {
                VaultDashboardViewModel => new DebugDashboardRepresentationControl(),
                _ => new TextBlock() { Text = "No content to show", HorizontalAlignment = HorizontalAlignment.Center }
            };
        }

        private void ShowHideDebuggingButton_Click(object sender, RoutedEventArgs e)
        {
            if (_rootControl?.DebugButton is null)
                return;

            _rootControl.DebugButton.Visibility = _rootControl.DebugButton.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }
    }
}
