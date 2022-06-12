using System.Collections.Generic;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.Services
{
    public interface ILocalizationService
    {
        AppLanguageModel CurrentAppLanguage { get; }

        string? LocalizeFromResourceKey(string resourceKey);

        AppLanguageModel? GetActiveLanguage();

        void SetActiveLanguage(AppLanguageModel language);

        IEnumerable<AppLanguageModel> GetLanguages();
    }
}
