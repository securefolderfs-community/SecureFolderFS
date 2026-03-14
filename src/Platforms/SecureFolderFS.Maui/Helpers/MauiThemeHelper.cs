using SecureFolderFS.UI.Enums;
using SecureFolderFS.UI.Helpers;

namespace SecureFolderFS.Maui.Helpers
{
    /// <inheritdoc cref="ThemeHelper"/>
    internal sealed class MauiThemeHelper : ThemeHelper
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="MauiThemeHelper"/>.
        /// </summary>
        public static MauiThemeHelper Instance => field ??= new();

        /// <inheritdoc/>
        public override event EventHandler? ActualThemeChanged;

        /// <inheritdoc/>
        public override ThemeType ActualTheme => (ThemeType)Application.Current!.RequestedTheme;

        private MauiThemeHelper()
        {
            Application.Current!.RequestedThemeChanged += Application_RequestedThemeChanged;
        }

        /// <inheritdoc/>
        protected override void UpdateTheme()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                App.Instance.UserAppTheme = (AppTheme)(int)CurrentTheme;
                ActualThemeChanged?.Invoke(this, EventArgs.Empty);
            });
        }

        private void Application_RequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
        {
            ActualThemeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
