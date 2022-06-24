using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.ViewModels.Settings.Banners;

namespace SecureFolderFS.Sdk.ViewModels.Pages.SettingsPages
{
    public sealed class GeneralSettingsPageViewModel : ObservableObject
    {
        private ILocalizationService LocalizationService { get; } = Ioc.Default.GetRequiredService<ILocalizationService>();

        public UpdateBannerViewModel BannerViewModel { get; }

        public ObservableCollection<AppLanguageModel> AppLanguages { get; }

        private bool _IsRestartRequired;
        public bool IsRestartRequired
        {
            get => _IsRestartRequired;
            set => SetProperty(ref _IsRestartRequired, value);
        }

        private int _SelectedLanguageIndex;
        public int SelectedLanguageIndex
        {
            get => _SelectedLanguageIndex;
            set
            {
                if (SetProperty(ref _SelectedLanguageIndex, value))
                {
                    LocalizationService.SetActiveLanguage(AppLanguages[value]);

                    IsRestartRequired = LocalizationService.CurrentAppLanguage.Id != AppLanguages[value].Id;
                }
            }
        }

        public GeneralSettingsPageViewModel()
        {
            BannerViewModel = new();
            AppLanguages = new(LocalizationService.GetLanguages());
        }
    }
}
