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
        private readonly ResourceLoader _resourceLoader;

        /// <inheritdoc/>
        public CultureInfo CurrentCulture { get; }

        /// <inheritdoc/>
        public IReadOnlyList<CultureInfo> AppLanguages { get; }

        public LocalizationService()
        {
            _resourceLoader = new();
            CurrentCulture = new(ApplicationLanguages.PrimaryLanguageOverride);
            AppLanguages = ApplicationLanguages.ManifestLanguages
                .Select(x => new CultureInfo(x))
                .ToList();
        }

        /// <inheritdoc/>
        public string? GetString(string resourceKey)
        {
            // TODO: Localize strings
            // return _resourceLoader.GetString(resourceKey);

            return resourceKey;
        }

        /// <inheritdoc/>
        public Task SetCultureAsync(CultureInfo cultureInfo)
        {
            ApplicationLanguages.PrimaryLanguageOverride = cultureInfo.Name;
            return Task.CompletedTask;
        }
    }
}
