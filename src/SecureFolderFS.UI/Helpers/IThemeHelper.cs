using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.Enums;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.UI.Helpers
{
    /// <summary>
    /// Represents a helper interface used for manipulating application themes.
    /// </summary>
    public interface IThemeHelper : IAsyncInitialize, INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the current theme used by the app.
        /// </summary>
        ThemeType CurrentTheme { get; }

        /// <summary>
        /// Updates the UI to reflect the new changes, if necessary.
        /// </summary>
        void UpdateTheme();

        /// <summary>
        /// Updates the application's theme to specified <paramref name="themeType"/>.
        /// </summary>
        /// <param name="themeType">The theme to set for the app.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task SetThemeAsync(ThemeType themeType, CancellationToken cancellationToken = default);
    }

    /// <inheritdoc cref="IThemeHelper"/>
    public interface IThemeHelper<out TImplementation> : IThemeHelper
        where TImplementation : IThemeHelper
    {
        /// <summary>
        /// Gets the singleton instance of <typeparamref name="TImplementation"/>.
        /// </summary>
        static abstract TImplementation Instance { get; }
    }
}
