using SecureFolderFS.Sdk.Services;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace SecureFolderFS.AvaloniaUI.ServiceImplementation
{
    // TODO: Implement localization
    /// <inheritdoc cref="ILocalizationService"/>
    internal sealed class LocalizationService : ILocalizationService
    {
        /// <inheritdoc/>
        public CultureInfo CurrentCulture { get; }

        /// <inheritdoc/>
        public IReadOnlyList<CultureInfo> AppLanguages { get; }

        public LocalizationService()
        {
            CurrentCulture = new("en-US");
            AppLanguages = new List<CultureInfo>() { CurrentCulture };
        }

        /// <inheritdoc/>
        public string? GetString(string resourceKey)
        {
            return resourceKey;
        }

        /// <inheritdoc/>
        public Task SetCultureAsync(CultureInfo cultureInfo)
        {
            return Task.CompletedTask;
        }
    }
}