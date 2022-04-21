using Microsoft.Windows.ApplicationModel.Resources;
using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.Services;
using System.Collections.Generic;
using System.Linq;
using Windows.Globalization;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    internal sealed class LocalizationService : ILocalizationService
    {
        private static readonly ResourceLoader IndependentLoader = new();

        private AppLanguageModel? _CurrentAppLanguage;
        public AppLanguageModel CurrentAppLanguage
        {
            get => _CurrentAppLanguage ??= new(ApplicationLanguages.PrimaryLanguageOverride);
        }

        public string LocalizeFromResourceKey(string resourceKey)
        {
            return IndependentLoader.GetString(resourceKey);
        }

        public AppLanguageModel? GetActiveLanguage()
        {
            var languages = GetLanguages();

            return languages.FirstOrDefault(item => item.Id == ApplicationLanguages.PrimaryLanguageOverride) ?? languages.FirstOrDefault();
        }

        public void SetActiveLanguage(AppLanguageModel language)
        {
            ApplicationLanguages.PrimaryLanguageOverride = language.Id;
        }

        public IEnumerable<AppLanguageModel> GetLanguages()
        {
            return ApplicationLanguages.ManifestLanguages.Select(item => new AppLanguageModel(item));
        }
    }
}
