using Microsoft.Windows.ApplicationModel.Resources;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using System.Collections.Generic;
using System.Linq;
using Windows.Globalization;
using SecureFolderFS.Sdk.AppModels;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="ILocalizationService"/>
    internal sealed class LocalizationService : ILocalizationService
    {
        private static readonly ResourceLoader ResourceLoader = new();
        private List<ILanguageModel>? _languageCache;

        public LocalizationService()
        {
            CurrentLanguage = new AppLanguageModel(ApplicationLanguages.PrimaryLanguageOverride);
        }

        /// <inheritdoc/>
        public ILanguageModel CurrentLanguage { get; }

        /// <inheritdoc/>
        public string? LocalizeString(string resourceKey)
        {
            return ResourceLoader.GetString(resourceKey);
        }

        /// <inheritdoc/>
        public void SetCurrentLanguage(ILanguageModel language)
        {
            ApplicationLanguages.PrimaryLanguageOverride = language.LanguageTag;
        }

        /// <inheritdoc/>
        public IEnumerable<ILanguageModel> GetLanguages()
        {
            _languageCache ??= ApplicationLanguages.ManifestLanguages
                .Select(item => new AppLanguageModel(item))
                .Cast<ILanguageModel>()
                .ToList();

            return _languageCache;
        }
    }
}
