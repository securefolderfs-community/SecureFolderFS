using SecureFolderFS.UI.Helpers;

namespace SecureFolderFS.Maui.Helpers
{
    /// <inheritdoc cref="ThemeHelper"/>
    internal sealed class MauiThemeHelper : ThemeHelper
    {
        private static MauiThemeHelper? _Instance;
        
        /// <summary>
        /// Gets the singleton instance of <see cref="MauiThemeHelper"/>.
        /// </summary>
        public static MauiThemeHelper Instance => _Instance ??= new();

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
