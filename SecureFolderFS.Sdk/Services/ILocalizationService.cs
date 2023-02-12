using SecureFolderFS.Sdk.Models;
using System.Collections.Generic;

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
        ILanguageModel CurrentLanguage { get; }

        /// <summary>
        /// Gets all supported app languages.
        /// </summary>
        IReadOnlyList<ILanguageModel> Languages { get; }

        /// <summary>
        /// Gets the localized version for the <paramref name="resourceKey"/>.
        /// </summary>
        /// <param name="resourceKey">The resource key that associates with translations.</param>
        /// <returns>A localized string for the <see cref="CurrentLanguage"/>.</returns>
        string? GetString(string resourceKey);

        /// <summary>
        /// Sets the current language of the app and updates <see cref="CurrentLanguage"/>.
        /// </summary>
        /// <param name="language">The language to set.</param>
        void SetCurrentLanguage(ILanguageModel language);
    }
}
