using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls.Banners;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Settings
{
    [Inject<ILocalizationService>, Inject<IApplicationService>]
    public sealed partial class GeneralSettingsViewModel : BasePageViewModel
    {
        public UpdateBannerViewModel BannerViewModel { get; }

        [ObservableProperty] private ObservableCollection<LanguageViewModel> _Languages;
        [ObservableProperty] private LanguageViewModel? _SelectedLanguage;
        [ObservableProperty] private bool _IsRestartRequired;

        public GeneralSettingsViewModel()
        {
            ServiceProvider = Ioc.Default;
            BannerViewModel = new();
            Languages = new();
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            foreach (var item in LocalizationService.AppLanguages)
                Languages.Add(new(item));

            // Set selected language
            SelectedLanguage = Languages.FirstOrDefault(x => x.CultureInfo.Equals(LocalizationService.CurrentCulture));

            // Initialize the banner
            await BannerViewModel.InitAsync(cancellationToken);
        }

        [RelayCommand]
        private async Task RestartAsync()
        {
            await ApplicationService.TryRestartAsync();
        }

        async partial void OnSelectedLanguageChanged(LanguageViewModel? value)
        {
            if (value is not null)
            {
                await LocalizationService.SetCultureAsync(value.CultureInfo);
                IsRestartRequired = !LocalizationService.CurrentCulture.Equals(SelectedLanguage?.CultureInfo);
            }
        }
    }
}
