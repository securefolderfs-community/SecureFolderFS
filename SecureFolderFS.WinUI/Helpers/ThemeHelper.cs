using System;
using System.Collections.Generic;
using CommunityToolkit.WinUI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.UI;
using Windows.UI.ViewManagement;
using Microsoft.UI.Dispatching;

#nullable enable

namespace SecureFolderFS.WinUI.Helpers
{
    internal class ThemeHelper
    {
        private readonly AppWindow _appWindow;

        private readonly UISettings _uiSettings;

        private readonly DispatcherQueue _dispatcherQueue;

        private readonly Dictionary<string, Action<ApplicationTheme>> _themeChangedCallbacks;

        public static ApplicationTheme CurrentTheme { get; private set; } = Application.Current.RequestedTheme;

        private static Dictionary<AppWindow, ThemeHelper> _ThemeHelpers { get; } = new();
        public static IReadOnlyDictionary<AppWindow, ThemeHelper> ThemeHelpers
        {
            get => _ThemeHelpers;
        }

        private ThemeHelper(AppWindow appWindow)
        {
            this._appWindow = appWindow;
            this._uiSettings = new();
            this._dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            this._themeChangedCallbacks = new();
            this._uiSettings.ColorValuesChanged += Settings_ColorValuesChanged;
        }

        private async void Settings_ColorValuesChanged(UISettings sender, object args)
        {
            CurrentTheme = CurrentTheme == ApplicationTheme.Dark ? ApplicationTheme.Light : ApplicationTheme.Dark;
            await _dispatcherQueue.EnqueueAsync(() =>
            {
                UpdateTheme();

                foreach (var item in _themeChangedCallbacks.Values)
                {
                    item(CurrentTheme);
                }
            });
        }

        public void UpdateTheme()
        {
            switch (CurrentTheme)
            {
                case ApplicationTheme.Dark:
                case ApplicationTheme.Light:
                    if (AppWindowTitleBar.IsCustomizationSupported())
                    {
                        _appWindow.TitleBar.ButtonHoverBackgroundColor = (Color)Application.Current.Resources["SystemBaseLowColor"];
                        _appWindow.TitleBar.ButtonForegroundColor = (Color)Application.Current.Resources["SystemBaseHighColor"];
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void RegisterForThemeChangeCallback(string className, Action<ApplicationTheme> callback)
        {
            _themeChangedCallbacks.Add(className, callback);
        }

        public void UnregisterForThemeChangeCallback(string className)
        {
            _themeChangedCallbacks.Remove(className);
        }

        public static ThemeHelper? RegisterWindowInstance(AppWindow appWindow)
        {
            var themeHelper = new ThemeHelper(appWindow);

            return _ThemeHelpers.TryAdd(appWindow, themeHelper) ? themeHelper : null;
        }

        public static bool UnregisterWindowInstance(AppWindow appWindow)
        {
            if (_ThemeHelpers.Remove(appWindow, out var themeHelper))
            {
                themeHelper._uiSettings.ColorValuesChanged -= themeHelper.Settings_ColorValuesChanged;
            }

            return false;
        }
    }
}
