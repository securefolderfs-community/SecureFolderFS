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
            var languageString = AppSettings.AppLanguage ?? ApplicationLanguages.PrimaryLanguageOverride;
            if (string.IsNullOrEmpty(languageString))
                return DefaultCulture;

            if (!ApplicationLanguages.ManifestLanguages.Contains(languageString))
                return DefaultCulture;

            return new(languageString);
        }

        /// <inheritdoc/>
        protected override IEnumerable<CultureInfo> GetAppLanguages()
        {
            // We need to deflate the SupportedLanguages list by ManifestLanguages
            foreach (var item in ApplicationLanguages.ManifestLanguages)
            {
                yield return new(SupportedLanguages.First(x => x.Contains(item)));
            }
        }
    }
}
