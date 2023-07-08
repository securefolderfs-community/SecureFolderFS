using SecureFolderFS.Sdk.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using Windows.Globalization;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="ILocalizationService"/>
    internal sealed class LocalizationService : ILocalizationService
    {
        private ResourceManager ResourceManager { get; set; }

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

            ResourceManager = new($"SecureFolderFS.UI.Strings.{GetLanguageString(CurrentCulture)}.Resources", typeof(UI.Constants).Assembly);
        }

        /// <inheritdoc/>
        public string? GetString(string resourceKey)
        {
            try
            {
                return ResourceManager.GetString(resourceKey);
            }
            catch (Exception ex)
            {
                _ = ex;
                Debugger.Break();

                return null;
            }
        }

        /// <inheritdoc/>
        public Task SetCultureAsync(CultureInfo cultureInfo)
        {
            ApplicationLanguages.PrimaryLanguageOverride = cultureInfo.Name;
            return Task.CompletedTask;
        }

        private static string GetLanguageString(CultureInfo cultureInfo)
        {
            if (cultureInfo.Name.Contains('-', StringComparison.OrdinalIgnoreCase))
                return cultureInfo.Name.Replace('-', '_');

            return $"{cultureInfo.Name}_{cultureInfo.TwoLetterISOLanguageName.ToUpperInvariant()}";
        }
    }
}
