using SecureFolderFS.Sdk.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="ILocalizationService"/>
    public abstract class BaseLocalizationService : ILocalizationService
    {
        protected static IReadOnlyList<string> SupportedLanguages { get; } = new List<string>()
        {
            "cz-CZ", "da-DK", "de-DE", "en-US", "es-ES", "fr-FR", "it-IT", "pl-PL", "ru-RU", "ua-UA"
        };

        protected abstract ResourceManager ResourceManager { get; }

        /// <inheritdoc/>
        public abstract CultureInfo CurrentCulture { get; }

        /// <inheritdoc/>
        public abstract IReadOnlyList<CultureInfo> AppLanguages { get; }

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
        public abstract Task SetCultureAsync(CultureInfo cultureInfo);

        protected static string GetLanguageString(CultureInfo cultureInfo)
        {
            if (cultureInfo.Name.Contains('-', StringComparison.OrdinalIgnoreCase))
                return cultureInfo.Name.Replace('-', '_');

            return SupportedLanguages.First(x => x.Contains(cultureInfo.Name)).Replace('-', '_');
        }
    }
}
