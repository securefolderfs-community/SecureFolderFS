using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.Shared;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="ILocalizationService"/>
    public class ResourceLocalizationService : ILocalizationService
    {
        protected static CultureInfo DefaultCulture { get; } = new(Constants.Application.DEFAULT_CULTURE_STRING);

        protected static IReadOnlyList<string> SupportedLanguages { get; } = new List<string>()
        {
            // Commented out languages exist in Resources but are not translated

            // "cz-CZ",
            "da-DK",
            "de-DE",
            "en-US",
            "es-ES",
            // "fr-FR", 
            // "he-IL",
            "hi-IN",
            "id-ID",
            // "it-IT",
            "ms-MY",
            "pl-PL",
            "pt-PT",
            // "ru-RU",
            "uk-UA",
        };

        protected IAppSettings AppSettings { get; } = DI.Service<ISettingsService>().AppSettings;

        protected virtual ResourceManager ResourceManager { get; }

        /// <inheritdoc/>
        public CultureInfo CurrentCulture { get; protected set; }

        /// <inheritdoc/>
        public IReadOnlyList<CultureInfo> AppLanguages { get; }

        public ResourceLocalizationService()
        {
            AppLanguages = GetAppLanguages().ToImmutableList();
            CurrentCulture = GetCurrentCulture();
            ResourceManager = new($"SecureFolderFS.UI.Strings.{GetLanguageString(CurrentCulture)}.Resources", typeof(UI.Constants).Assembly);
        }

        /// <inheritdoc/>
        public virtual string? TryGetString(string resourceKey)
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
        public virtual Task SetCultureAsync(CultureInfo cultureInfo)
        {
            CurrentCulture = cultureInfo;
            AppSettings.AppLanguage = cultureInfo.Name;

            return AppSettings.SaveAsync();
        }

        protected virtual IEnumerable<CultureInfo> GetAppLanguages()
        {
            foreach (var item in SupportedLanguages)
            {
                yield return new(item);
            }   
        }

        protected virtual CultureInfo GetCurrentCulture()
        {
            if (string.IsNullOrEmpty(AppSettings.AppLanguage))
                return DefaultCulture;

            if (!SupportedLanguages.Contains(AppSettings.AppLanguage))
                return DefaultCulture;

            return new(AppSettings.AppLanguage);
        }

        protected virtual string GetLanguageString(CultureInfo cultureInfo)
        {
            if (cultureInfo.Name.Contains('-', StringComparison.OrdinalIgnoreCase))
                return cultureInfo.Name.Replace('-', '_');

            return SupportedLanguages.First(x => x.Contains(cultureInfo.Name)).Replace('-', '_');
        }
    }
}
