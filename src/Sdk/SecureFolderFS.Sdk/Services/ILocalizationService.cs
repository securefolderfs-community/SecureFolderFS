using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// A service that manages localization of the app.
    /// </summary>
    public interface ILocalizationService : IResourceLocator<string?>
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
        /// Sets the current language of the app.
        /// </summary>
        /// <param name="cultureInfo">The language to set.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task SetCultureAsync(CultureInfo cultureInfo);
    }
}
