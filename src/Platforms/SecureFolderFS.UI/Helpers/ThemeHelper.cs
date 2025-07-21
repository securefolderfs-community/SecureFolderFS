using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.Enums;

namespace SecureFolderFS.UI.Helpers
{
    /// <summary>
    /// Represents a helper class used for manipulating application themes.
    /// </summary>
    public abstract class ThemeHelper : ObservableObject, IAsyncInitialize
    {
        protected IAppSettings AppSettings { get; } = DI.Service<ISettingsService>().AppSettings;

        private ThemeType _CurrentTheme;
        /// <summary>
        /// Gets the current theme used by the app.
        /// </summary>
        public virtual ThemeType CurrentTheme
        {
            get => _CurrentTheme;
            protected set => SetProperty(ref _CurrentTheme, value);
        }

        /// <summary>
        /// Updates the application's theme to specified <paramref name="themeType"/>.
        /// </summary>
        /// <param name="themeType">The theme to set for the app.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public Task SetThemeAsync(ThemeType themeType, CancellationToken cancellationToken = default)
        {
            CurrentTheme = themeType;
            AppSettings.ApplicationTheme = ConvertThemeType(themeType);

            UpdateTheme();
            return AppSettings.TrySaveAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual Task InitAsync(CancellationToken cancellationToken = default)
        {
            CurrentTheme = ConvertThemeString(AppSettings.ApplicationTheme);
            UpdateTheme();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Updates the UI to reflect the new changes, if necessary.
        /// </summary>
        protected abstract void UpdateTheme();

        protected static string? ConvertThemeType(ThemeType themeType)
        {
            return themeType switch
            {
                ThemeType.Light => Constants.AppThemes.LIGHT_THEME,
                ThemeType.Dark => Constants.AppThemes.DARK_THEME,
                _ => null
            };
        }

        protected static ThemeType ConvertThemeString(string? themeString)
        {
            return themeString switch
            {
                Constants.AppThemes.LIGHT_THEME => ThemeType.Light,
                Constants.AppThemes.DARK_THEME => ThemeType.Dark,
                _ => ThemeType.Default
            };
        }
    }
}
