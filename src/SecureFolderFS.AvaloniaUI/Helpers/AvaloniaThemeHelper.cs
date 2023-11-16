using Avalonia;
using Avalonia.Styling;
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
            _fluentAvaloniaTheme.PreferSystemTheme = CurrentTheme == ThemeType.Default;
            if (_fluentAvaloniaTheme.PreferSystemTheme)
                return;

            Application.Current!.RequestedThemeVariant = CurrentTheme switch
            {
                ThemeType.Light => ThemeVariant.Light,
                _ => ThemeVariant.Dark,
            };
        }
    }
}