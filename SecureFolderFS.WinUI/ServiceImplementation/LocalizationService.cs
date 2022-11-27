using Microsoft.Windows.ApplicationModel.Resources;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using System.Collections.Generic;
using System.Linq;
using Windows.Globalization;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="ILocalizationService"/>
    internal sealed class LocalizationService : ILocalizationService
    {
        private static readonly ResourceLoader ResourceLoader = new();
        private IReadOnlyList<ILanguageModel>? _languageCache;

        public LocalizationService()
        {
            CurrentLanguage = new AppLanguageModel(ApplicationLanguages.PrimaryLanguageOverride);
        }

        /// <inheritdoc/>
        public ILanguageModel CurrentLanguage { get; }

        /// <inheritdoc/>
        public string? LocalizeString(string resourceKey)
        {
            return resourceKey;

            // TODO: Localize strings
            // return ResourceLoader.GetString(resourceKey);
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
                .Select<string, ILanguageModel>(item => new AppLanguageModel(item))
                .ToList();

            return _languageCache;
        }
    }
}
