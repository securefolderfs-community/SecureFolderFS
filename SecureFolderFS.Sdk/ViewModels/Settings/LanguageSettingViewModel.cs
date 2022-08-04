using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;

namespace SecureFolderFS.Sdk.ViewModels.Settings
{
    public sealed partial class LanguageSettingViewModel : ObservableObject
    {
        private ILocalizationService LocalizationService { get; } = Ioc.Default.GetRequiredService<ILocalizationService>();

        public ObservableCollection<LanguageViewModel> Languages { get; }

        public ILanguageModel ActiveLanguage { get; }

        [ObservableProperty]
        private bool _IsRestartRequired;

        public LanguageSettingViewModel()
        {
            Languages = new(LocalizationService.GetLanguages().Select(x => new LanguageViewModel(x)));
            ActiveLanguage = LocalizationService.CurrentLanguage;
        }

        public void UpdateCurrentLanguage(ILanguageModel language)
        {
            LocalizationService.SetCurrentLanguage(language);
            IsRestartRequired = !ActiveLanguage.LanguageTag.Equals(LocalizationService.CurrentLanguage.LanguageTag);
        }
    }
}
