using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.SettingsPersistence;
using SecureFolderFS.UI.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.UI.Helpers
{
    /// <inheritdoc cref="IThemeHelper"/>
    public abstract class ThemeHelper<TImplementation> : ObservableObject, IThemeHelper
        where TImplementation : IThemeHelper<TImplementation>
    {
        protected IAppSettings AppSettings { get; } = Ioc.Default.GetRequiredService<ISettingsService>().AppSettings;

        private ThemeType _CurrentTheme;
        public virtual ThemeType CurrentTheme
        {
            get => _CurrentTheme;
            protected set => SetProperty(ref _CurrentTheme, value);
        }

        /// <inheritdoc/>
        public abstract void UpdateTheme();

        /// <inheritdoc/>
        public Task SetThemeAsync(ThemeType themeType, CancellationToken cancellationToken = default)
        {
            CurrentTheme = themeType;
            AppSettings.ApplicationTheme = ConvertThemeType(themeType);

            UpdateTheme();
            return AppSettings.SaveAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual Task InitAsync(CancellationToken cancellationToken = default)
        {
            CurrentTheme = ConvertThemeString(AppSettings.ApplicationTheme);
            UpdateTheme();

            return Task.CompletedTask;
        }

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
