using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using SecureFolderFS.UI.Enums;
using SecureFolderFS.UI.Helpers;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace SecureFolderFS.WinUI.Helpers
{
    /// <inheritdoc cref="IThemeHelper"/>
    internal sealed class WinUIThemeHelper : ThemeHelper<WinUIThemeHelper>, IThemeHelper<WinUIThemeHelper>
    {
        private AppWindow? _appWindow;
        private FrameworkElement? _rootContent;
        private readonly UISettings _uiSettings;
        private readonly DispatcherQueue _dispatcherQueue;

        /// <inheritdoc/>
        public static WinUIThemeHelper Instance { get; } = new();

        private WinUIThemeHelper()
        {
            _uiSettings = new();
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            _uiSettings.ColorValuesChanged += Settings_ColorValuesChanged;
        }

        /// <inheritdoc/>
        public override Task SetThemeAsync(ThemeType themeType, CancellationToken cancellationToken = default)
        {
            if (_rootContent is not null)
            {
                if (themeType == ThemeType.Default)
                    _rootContent.RequestedTheme = Application.Current.RequestedTheme == ApplicationTheme.Dark ? ElementTheme.Dark : ElementTheme.Light;
                else
                    _rootContent.RequestedTheme = (ElementTheme)(uint)themeType;
            }

            if (_appWindow is not null && AppWindowTitleBar.IsCustomizationSupported())
            {
                switch (themeType)
                {
                    case ThemeType.Dark:
                        _appWindow.TitleBar.ButtonForegroundColor = Color.FromArgb(255, 255, 255, 255);
                        _appWindow.TitleBar.ButtonHoverBackgroundColor = Color.FromArgb(21, 255, 255, 255);
                        _appWindow.TitleBar.ButtonHoverForegroundColor = Color.FromArgb(255, 255, 255, 255);
                        _appWindow.TitleBar.ButtonPressedBackgroundColor = Color.FromArgb(8, 255, 255, 255);
                        _appWindow.TitleBar.ButtonPressedForegroundColor = Color.FromArgb(255, 255, 255, 255);
                        break;

                    case ThemeType.Light:
                        _appWindow.TitleBar.ButtonForegroundColor = Color.FromArgb(228, 0, 0, 0);
                        _appWindow.TitleBar.ButtonHoverBackgroundColor = Color.FromArgb(128, 249, 249, 249);
                        _appWindow.TitleBar.ButtonHoverForegroundColor = Color.FromArgb(255, 0, 0, 0);
                        _appWindow.TitleBar.ButtonPressedBackgroundColor = Color.FromArgb(77, 249, 249, 249);
                        _appWindow.TitleBar.ButtonPressedForegroundColor = Color.FromArgb(255, 0, 0, 0);
                        break;

                    default:
                        _appWindow.TitleBar.ButtonForegroundColor = ((SolidColorBrush?)Application.Current.Resources["ButtonForeground"])?.Color;
                        _appWindow.TitleBar.ButtonHoverBackgroundColor = ((SolidColorBrush?)Application.Current.Resources["ButtonBackgroundPointerOver"])?.Color;
                        _appWindow.TitleBar.ButtonHoverForegroundColor = ((SolidColorBrush?)Application.Current.Resources["ButtonForegroundPointerOver"])?.Color;
                        _appWindow.TitleBar.ButtonPressedBackgroundColor = ((SolidColorBrush?)Application.Current.Resources["ButtonBackgroundPressed"])?.Color;
                        _appWindow.TitleBar.ButtonPressedForegroundColor = ((SolidColorBrush?)Application.Current.Resources["ButtonForegroundPressed"])?.Color;
                        break;
                }
            }

            return base.SetThemeAsync(themeType, cancellationToken);
        }

        public void RegisterWindowInstance(AppWindow appWindow, FrameworkElement? rootContent)
        {
            _appWindow = appWindow;
            _rootContent = rootContent;
        }

        private async void Settings_ColorValuesChanged(UISettings sender, object args)
        {
            await SetThemeAsync(CurrentTheme);
        }
    }
}
