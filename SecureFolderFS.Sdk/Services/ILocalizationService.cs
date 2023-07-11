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
        /// Tries to gets the localized string for the <paramref name="resourceKey"/>.
        /// </summary>
        /// <param name="resourceKey">The resource key that associates with translations.</param>
        /// <returns>If successful, returns a localized string for the <see cref="CurrentCulture"/>; otherwise null.</returns>
        string? TryGetString(string resourceKey);

        /// <summary>
        /// Sets the current language of the app.
        /// </summary>
        /// <remarks>
        /// This method does not update <see cref="CurrentCulture"/> with the new value.
        /// </remarks>
        /// <param name="cultureInfo">The language to set.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task SetCultureAsync(CultureInfo cultureInfo);
    }
}
