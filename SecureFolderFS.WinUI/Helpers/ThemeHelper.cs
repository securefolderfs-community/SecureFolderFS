using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace SecureFolderFS.WinUI.Helpers
{
    internal sealed class ThemeHelper : ObservableObject
    {
        private AppWindow? _appWindow;
        private FrameworkElement? _rootContent;
        private readonly UISettings _uiSettings;
        private readonly DispatcherQueue _dispatcherQueue;

        public static ThemeHelper Instance { get; } = new();

        public event EventHandler<ElementTheme>? OnThemeChangedEvent;

        private ElementTheme _CurrentTheme;
        public ElementTheme CurrentTheme
        {
            get => _CurrentTheme;
            set
            {
                if (SetProperty(ref _CurrentTheme, value))
                {
                    _CurrentTheme = value;
                    ApplicationData.Current.LocalSettings.Values[Constants.AppLocalSettings.THEME_PREFERENCE_SETTING] = (int)value;
                    UpdateTheme();
                }
            }
        }

        private ThemeHelper()
        {
            _uiSettings = new();
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            _uiSettings.ColorValuesChanged += Settings_ColorValuesChanged;
            _CurrentTheme = ((int?)ApplicationData.Current.LocalSettings.Values[Constants.AppLocalSettings.THEME_PREFERENCE_SETTING] ?? 0) switch
            {
                1 => ElementTheme.Light,
                2 => ElementTheme.Dark,
                _ => ElementTheme.Default
            };
        }

        public void UpdateTheme()
        {
            if (_rootContent is not null)
            {
                if (CurrentTheme == ElementTheme.Default)
                    _rootContent.RequestedTheme = Application.Current.RequestedTheme == ApplicationTheme.Dark ? ElementTheme.Dark : ElementTheme.Light;
                else
                    _rootContent.RequestedTheme = CurrentTheme;
            }

            if (_appWindow is not null && AppWindowTitleBar.IsCustomizationSupported())
            {
                switch (CurrentTheme)
                {
                    case ElementTheme.Dark:
                        _appWindow.TitleBar.ButtonForegroundColor = Color.FromArgb(255, 255, 255, 255);
                        _appWindow.TitleBar.ButtonHoverBackgroundColor = Color.FromArgb(21, 255, 255, 255);
                        _appWindow.TitleBar.ButtonHoverForegroundColor = Color.FromArgb(255, 255, 255, 255);
                        _appWindow.TitleBar.ButtonPressedBackgroundColor = Color.FromArgb(8, 255, 255, 255);
                        _appWindow.TitleBar.ButtonPressedForegroundColor = Color.FromArgb(255, 255, 255, 255);
                        break;

                    case ElementTheme.Light:
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
        }

        public void RegisterWindowInstance(AppWindow appWindow, FrameworkElement rootContent)
        {
            _appWindow = appWindow;
            _rootContent = rootContent;
            UpdateTheme();
        }

        private async void Settings_ColorValuesChanged(UISettings sender, object args)
        {
            await _dispatcherQueue.EnqueueAsync(() =>
            {
                UpdateTheme();
                OnThemeChangedEvent?.Invoke(this, _CurrentTheme);
            }, DispatcherQueuePriority.Low);
        }
    }
}
