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
        private readonly ResourceLoader _resourceLoader;

        /// <inheritdoc/>
        public IReadOnlyList<ILanguageModel> Languages { get; }

        /// <inheritdoc/>
        public ILanguageModel CurrentLanguage { get; }

        public LocalizationService()
        {
            _resourceLoader = new();
            CurrentLanguage = new AppLanguageModel(ApplicationLanguages.PrimaryLanguageOverride);
            Languages = ApplicationLanguages.ManifestLanguages
                .Select<string, ILanguageModel>(x => new AppLanguageModel(x))
                .ToList();
        }

        /// <inheritdoc/>
        public string? GetString(string resourceKey)
        {
            return resourceKey;

            // TODO: Localize strings
            // return _resourceLoader.GetString(resourceKey);
        }

        /// <inheritdoc/>
        public void SetCurrentLanguage(ILanguageModel language)
        {
            ApplicationLanguages.PrimaryLanguageOverride = language.LanguageTag;
        }
    }
}
