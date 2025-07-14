using SecureFolderFS.UI.Helpers;

namespace SecureFolderFS.Maui.Helpers
{
    /// <inheritdoc cref="ThemeHelper"/>
    internal sealed class MauiThemeHelper : ThemeHelper
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="MauiThemeHelper"/>.
        /// </summary>
        public static MauiThemeHelper Instance { get; } = new();

        /// <inheritdoc/>
        protected override void UpdateTheme()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                App.Instance.UserAppTheme = (AppTheme)(int)CurrentTheme;
            });
        }
    }
}
