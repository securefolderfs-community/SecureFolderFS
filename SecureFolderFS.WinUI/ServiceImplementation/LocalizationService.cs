using Microsoft.Windows.ApplicationModel.Resources;
using SecureFolderFS.Sdk.Services;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.Globalization;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    // TODO: Implement localization
    /// <inheritdoc cref="ILocalizationService"/>
    internal sealed class LocalizationService : ILocalizationService
    {
        private static ResourceLoader ResourceLoader { get; } = new();

        /// <inheritdoc/>
        public CultureInfo CurrentCulture { get; }

        /// <inheritdoc/>
        public IReadOnlyList<CultureInfo> AppLanguages { get; }

        public LocalizationService()
        {
            CurrentCulture = new(ApplicationLanguages.PrimaryLanguageOverride);
            AppLanguages = ApplicationLanguages.ManifestLanguages
                .Select(x => new CultureInfo(x))
                .ToList();
        }

        /// <inheritdoc/>
        public string? GetString(string resourceKey)
        {
            return ResourceLoader.GetString(resourceKey);
        }

        /// <inheritdoc/>
        public Task SetCultureAsync(CultureInfo cultureInfo)
        {
            ApplicationLanguages.PrimaryLanguageOverride = cultureInfo.Name;
            return Task.CompletedTask;
        }
    }
}
