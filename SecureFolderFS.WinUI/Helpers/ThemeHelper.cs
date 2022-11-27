using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.UI.ViewManagement;

namespace SecureFolderFS.WinUI.Helpers
{
    internal sealed class ThemeHelper : ObservableObject
    {
        private AppWindow? _appWindow;
        private FrameworkElement? _rootContent;
        private readonly UISettings _uiSettings;
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly Dictionary<string, Action<ElementTheme>> _themeChangedCallbacks;

        public static ThemeHelper Instance { get; } = new();

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
            _themeChangedCallbacks = new();
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
                _appWindow.TitleBar.ButtonHoverBackgroundColor = ((SolidColorBrush?)Application.Current.Resources["ButtonBackgroundPointerOver"])?.Color;
                _appWindow.TitleBar.ButtonForegroundColor = ((SolidColorBrush?)Application.Current.Resources["ButtonForeground"])?.Color;
            }
        }

        public void SubscribeThemeChangedCallback(string className, Action<ElementTheme> callback)
        {
            _themeChangedCallbacks.Add(className, callback);
        }

        public void UnsubscribeThemeChangedCallback(string className)
        {
            _themeChangedCallbacks.Remove(className);
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
                foreach (var item in _themeChangedCallbacks.Values)
                {
                    item(CurrentTheme);
                }
            }, DispatcherQueuePriority.Low);
        }
    }
}
