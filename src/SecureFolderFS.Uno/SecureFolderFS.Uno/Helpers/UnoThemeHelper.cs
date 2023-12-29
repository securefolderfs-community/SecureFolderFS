using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using SecureFolderFS.UI.Enums;
using SecureFolderFS.UI.Helpers;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace SecureFolderFS.Uno.Helpers
{
    /// <inheritdoc cref="ThemeHelper"/>
    internal sealed class UnoThemeHelper : ThemeHelper
    {
        private AppWindow? _appWindow;
        private FrameworkElement? _rootContent;
        private readonly UISettings _uiSettings;
        private readonly DispatcherQueue _dispatcherQueue;

        /// <summary>
        /// Gets the singleton instance of <see cref="UnoThemeHelper"/>.
        /// </summary>
        public static UnoThemeHelper Instance { get; } = new();

        /// <summary>
        /// Gets the current theme represented by <see cref="ElementTheme"/>.
        /// </summary>
        public ElementTheme CurrentElementTheme => (ElementTheme)(uint)CurrentTheme;

        /// <inheritdoc/>
        public override ThemeType CurrentTheme
        {
            get => base.CurrentTheme;
            protected set
            {
                base.CurrentTheme = value;
                OnPropertyChanged(nameof(CurrentElementTheme));
            }
        }

        private UnoThemeHelper()
        {
            _uiSettings = new();
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            _uiSettings.ColorValuesChanged += Settings_ColorValuesChanged;
        }

        /// <inheritdoc/>
        public override void UpdateTheme()
        {
            if (_rootContent is not null)
            {
                if (CurrentTheme == ThemeType.Default)
                    _rootContent.RequestedTheme = Application.Current.RequestedTheme == ApplicationTheme.Dark ? ElementTheme.Dark : ElementTheme.Light;
                else
                    _rootContent.RequestedTheme = (ElementTheme)(uint)CurrentTheme;
            }

#if WINDOWS
            if (_appWindow is not null && AppWindowTitleBar.IsCustomizationSupported())
            {
                switch (CurrentTheme)
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

                    case ThemeType.Default:
                    default:
                        _appWindow.TitleBar.ButtonForegroundColor = ((SolidColorBrush?)Application.Current.Resources["ButtonForeground"])?.Color;
                        _appWindow.TitleBar.ButtonHoverBackgroundColor = ((SolidColorBrush?)Application.Current.Resources["ButtonBackgroundPointerOver"])?.Color;
                        _appWindow.TitleBar.ButtonHoverForegroundColor = ((SolidColorBrush?)Application.Current.Resources["ButtonForegroundPointerOver"])?.Color;
                        _appWindow.TitleBar.ButtonPressedBackgroundColor = ((SolidColorBrush?)Application.Current.Resources["ButtonBackgroundPressed"])?.Color;
                        _appWindow.TitleBar.ButtonPressedForegroundColor = ((SolidColorBrush?)Application.Current.Resources["ButtonForegroundPressed"])?.Color;
                        break;
                }
            }
#endif
        }

        public void RegisterWindowInstance(FrameworkElement? rootContent, AppWindow? appWindow = null)
        {
            _rootContent = rootContent;
            _appWindow = appWindow;
        }

        private void Settings_ColorValuesChanged(UISettings sender, object args)
        {
            _ = _dispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, UpdateTheme);
        }
    }
}
