using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.ViewModels.Settings;
using SecureFolderFS.Sdk.ViewModels.Settings.Banners;

namespace SecureFolderFS.Sdk.ViewModels.Pages.SettingsPages
{
    public sealed class GeneralSettingsPageViewModel : ObservableObject
    {
        private ILocalizationService LocalizationService { get; } = Ioc.Default.GetRequiredService<ILocalizationService>();

        public UpdateBannerViewModel BannerViewModel { get; }

        public LanguageSettingViewModel LanguageSettingViewModel { get; }

        public GeneralSettingsPageViewModel()
        {
            BannerViewModel = new();
            LanguageSettingViewModel = new();
        }
    }
}
