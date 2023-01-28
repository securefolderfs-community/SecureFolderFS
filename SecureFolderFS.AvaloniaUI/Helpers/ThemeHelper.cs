using System;
using System.Reactive;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using FluentAvalonia.Styling;
using SecureFolderFS.AvaloniaUI.Events;
using SecureFolderFS.AvaloniaUI.Services;
using SecureFolderFS.UI.Enums;

namespace SecureFolderFS.AvaloniaUI.Helpers
{
    internal sealed class ThemeHelper : ObservableObject
    {
        public static ThemeHelper Instance { get; } = new();

        public event EventHandler<GenericEventArgs<ApplicationTheme>>? OnThemeChangedEvent;

        private readonly IPlatformSettingsService _platformSettingsService;
        private readonly FluentAvaloniaTheme _fluentAvaloniaTheme;


        private ApplicationTheme _CurrentTheme;
        public ApplicationTheme CurrentTheme
        {
            get => _CurrentTheme;
            set
            {
                if (SetProperty(ref _CurrentTheme, value))
                {
                    _platformSettingsService.Theme = value;
                    UpdateTheme();

                    OnThemeChangedEvent?.Invoke(this, new(value));
                }
            }
        }

        private ThemeHelper()
        {
            _platformSettingsService = Ioc.Default.GetRequiredService<IPlatformSettingsService>();
            _fluentAvaloniaTheme = AvaloniaLocator.Current.GetRequiredService<FluentAvaloniaTheme>();
            _CurrentTheme = _platformSettingsService.Theme;

            _fluentAvaloniaTheme.PreferSystemTheme = _platformSettingsService.Theme == ApplicationTheme.Default;
        }

        public void UpdateTheme()
        {
            if (_platformSettingsService.Theme == ApplicationTheme.Default)
            {
                _fluentAvaloniaTheme.PreferSystemTheme = true;
                _fluentAvaloniaTheme.InvalidateThemingFromSystemThemeChanged();
                return;
            }

            _fluentAvaloniaTheme.PreferSystemTheme = false;
            _fluentAvaloniaTheme.RequestedTheme = _platformSettingsService.Theme switch
            {
                ApplicationTheme.Light => FluentAvaloniaTheme.LightModeString,
                _ => FluentAvaloniaTheme.DarkModeString,
            };
        }
    }
}