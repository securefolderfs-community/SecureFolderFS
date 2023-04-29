using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// A service that manages localization of the app.
    /// </summary>
    public interface ILocalizationService
    {
        /// <summary>
        /// Gets the current language of the app.
        /// </summary>
        CultureInfo CurrentCulture { get; }

        /// <summary>
        /// Gets all supported languages by the app.
        /// </summary>
        IReadOnlyList<CultureInfo> AppLanguages { get; }

        /// <summary>
        /// Gets the localized version for the <paramref name="resourceKey"/>.
        /// </summary>
        /// <param name="resourceKey">The resource key that associates with translations.</param>
        /// <returns>A localized string for the <see cref="CurrentCulture"/>.</returns>
        string? GetString(string resourceKey);

        /// <summary>
        /// Sets the current language of the app and updates <see cref="CurrentCulture"/>.
        /// </summary>
        /// <param name="cultureInfo">The language to set.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task SetCultureAsync(CultureInfo cultureInfo);
    }
}
