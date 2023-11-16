using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.ServiceImplementation;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.Globalization;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="ILocalizationService"/>
    internal sealed class PackageLocalizationService : ResourceLocalizationService
    {
        /// <inheritdoc/>
        public override Task SetCultureAsync(CultureInfo cultureInfo)
        {
            ApplicationLanguages.PrimaryLanguageOverride = cultureInfo.Name;
            return base.SetCultureAsync(cultureInfo);
        }

        /// <inheritdoc/>
        protected override CultureInfo GetCurrentCulture()
        {
            // PrimaryLanguageOverride may return an empty string and thus it is better to use null as the "empty" equivalent
            var primaryLanguageOverride = string.IsNullOrEmpty(ApplicationLanguages.PrimaryLanguageOverride) ? null : ApplicationLanguages.PrimaryLanguageOverride;

            // Prefer PrimaryLanguageOverride as the default language identifier here since it provides
            // package compliant language identifiers. However, PrimaryLanguageOverride holds less information
            // about a specific culture than AppLanguage property. Oftentimes the 'country' identifier
            // is removed from the name part of a culture when PrimaryLanguageOverride is used.
            // For example:
            // PrimaryLanguageOverride may save an identifier as: "uk"
            // While AppLanguage may save the fully-qualified identifier: "uk-UA"
            var languageString = primaryLanguageOverride ?? AppSettings.AppLanguage;
            if (string.IsNullOrEmpty(languageString))
                return DefaultCulture;

            // By getting the identifier from PrimaryLanguageOverride we then check against ManifestLanguages,
            // to determine whether or not that specified language exists. Instead of checking if ManifestLanguages
            // contains the specified languageString, we instead need to check whether that languageString starts with
            // an item from ManifestLanguages. This is due to the fact, that ManifestLanguages sometimes loses information
            // about the 'country' identifier whilst languageString may not (in case AppLanguage was chosen)
            // and therefore wouldn't yield any results.
            if (!ApplicationLanguages.ManifestLanguages.Aggregate(false, (current, item) => current | languageString.StartsWith(item)))
                return DefaultCulture;

            // To get the fully-qualified language identifier, and therefore the most compatible one,
            // we need to again perform the null-coalescing expression, however, this time the null-check
            // order is swapped and consequently AppLanguage property is more preferred than PrimaryLanguageOverride.
            // By doing the swap, we ensure that the most compliant identifier is returned.
            languageString = AppSettings.AppLanguage ?? primaryLanguageOverride;
            if (string.IsNullOrEmpty(languageString))
                return DefaultCulture;

            return new(languageString);
        }

        /// <inheritdoc/>
        protected override IEnumerable<CultureInfo> GetAppLanguages()
        {
            // We need to deflate the SupportedLanguages list by ManifestLanguages
            foreach (var item in ApplicationLanguages.ManifestLanguages)
            {
                // Instead of returning entries of ManifestLanguages, we return our own
                // entries of SupportedLanguages that fully qualify each language
                yield return new(SupportedLanguages.First(x => x.Contains(item)));
            }
        }
    }
}
