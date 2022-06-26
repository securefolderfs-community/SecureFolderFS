using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Sdk.ViewModels.Settings
{
    public sealed class LanguageSettingViewModel : ObservableObject
    {
        private readonly ILanguageModel _activeLanguage;

        private ILocalizationService LocalizationService { get; } = Ioc.Default.GetRequiredService<ILocalizationService>();

        public ObservableCollection<ILanguageModel> Languages { get; }

        private bool _IsRestartRequired;
        public bool IsRestartRequired
        {
            get => _IsRestartRequired;
            set => SetProperty(ref _IsRestartRequired, value);
        }

        public LanguageSettingViewModel()
        {
            Languages = new(LocalizationService.GetLanguages());
            _activeLanguage = LocalizationService.CurrentLanguage;
        }

        public void UpdateCurrentLanguage(ILanguageModel language)
        {
            LocalizationService.SetCurrentLanguage(language);
            IsRestartRequired = !_activeLanguage.LanguageTag.Equals(LocalizationService.CurrentLanguage.LanguageTag);
        }
    }
}
