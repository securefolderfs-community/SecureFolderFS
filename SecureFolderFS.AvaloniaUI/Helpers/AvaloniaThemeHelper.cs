using Avalonia;
using FluentAvalonia.Styling;
using SecureFolderFS.UI.Enums;
using SecureFolderFS.UI.Helpers;
using System.Threading;
using System.Threading.Tasks;

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
        public override async Task SetThemeAsync(ThemeType themeType, CancellationToken cancellationToken = default)
        {
            if (themeType == ThemeType.Default)
            {
                _fluentAvaloniaTheme.PreferSystemTheme = true;
                _fluentAvaloniaTheme.InvalidateThemingFromSystemThemeChanged();
                return;
            }

            _fluentAvaloniaTheme.PreferSystemTheme = false;
            _fluentAvaloniaTheme.RequestedTheme = themeType switch
            {
                ThemeType.Light => FluentAvaloniaTheme.LightModeString,
                _ => FluentAvaloniaTheme.DarkModeString,
            };

            await base.SetThemeAsync(themeType, cancellationToken);
        }
    }
}