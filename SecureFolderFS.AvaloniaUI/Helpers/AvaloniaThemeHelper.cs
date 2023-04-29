using Avalonia;
using FluentAvalonia.Styling;
using SecureFolderFS.UI.Enums;
using SecureFolderFS.UI.Helpers;

namespace SecureFolderFS.AvaloniaUI.Helpers
{
    /// <inheritdoc cref="IThemeHelper"/>
    internal sealed class AvaloniaThemeHelper : ThemeHelper<AvaloniaThemeHelper>, IThemeHelper<AvaloniaThemeHelper>
    {
        private readonly FluentAvaloniaTheme _fluentAvaloniaTheme;

        /// <inheritdoc/>
        public static AvaloniaThemeHelper Instance { get; } = new();

        private AvaloniaThemeHelper()
        {
            _fluentAvaloniaTheme = AvaloniaLocator.Current.GetRequiredService<FluentAvaloniaTheme>();
            _fluentAvaloniaTheme.PreferSystemTheme = CurrentTheme == ThemeType.Default;
        }

        /// <inheritdoc/>
        public override void UpdateTheme()
        {
            if (CurrentTheme == ThemeType.Default)
            {
                _fluentAvaloniaTheme.PreferSystemTheme = true;
                _fluentAvaloniaTheme.InvalidateThemingFromSystemThemeChanged();
                return;
            }

            _fluentAvaloniaTheme.PreferSystemTheme = false;
            _fluentAvaloniaTheme.RequestedTheme = CurrentTheme switch
            {
                ThemeType.Light => FluentAvaloniaTheme.LightModeString,
                _ => FluentAvaloniaTheme.DarkModeString,
            };
        }
    }
}