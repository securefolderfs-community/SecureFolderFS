using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls.Banners;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Settings
{
    [Inject<ILocalizationService>, Inject<IApplicationService>]
    public sealed partial class GeneralSettingsViewModel : BasePageViewModel
    {
        private readonly CultureInfo _currentCulture;
        private bool _noNotify;

        [ObservableProperty] private ObservableCollection<LanguageViewModel> _Languages;
        [ObservableProperty] private LanguageViewModel? _SelectedLanguage;
        [ObservableProperty] private bool _IsRestartRequired;

        public UpdateBannerViewModel BannerViewModel { get; }

        public GeneralSettingsViewModel()
        {
            ServiceProvider = Ioc.Default;
            BannerViewModel = new();
            Languages = new();
            
            _currentCulture = LocalizationService.CurrentCulture;
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            foreach (var item in LocalizationService.AppLanguages)
                Languages.Add(new(item));

            // Add wildcard language
            Languages.Add(new(CultureInfo.InvariantCulture, "Not seeing your language?"));

            // Set selected language
            _noNotify = true;
            SelectedLanguage = Languages.FirstOrDefault(x => x.CultureInfo.Equals(LocalizationService.CurrentCulture));
            _noNotify = false;

            // Initialize the banner
            await BannerViewModel.InitAsync(cancellationToken);
        }

        [RelayCommand]
        private Task RestartAsync()
        {
            return ApplicationService.TryRestartAsync();
        }

        async partial void OnSelectedLanguageChanged(LanguageViewModel? value)
        {
            if (value is null || _noNotify)
                return;

            if (value.CultureInfo.Equals(CultureInfo.InvariantCulture))
            {
                // Wildcard
                await ApplicationService.OpenUriAsync(new("https://github.com/securefolderfs-community/SecureFolderFS/issues/50"));

                SelectedLanguage = Languages.FirstOrDefault(x => x.CultureInfo.Equals(LocalizationService.CurrentCulture));
                if (SelectedLanguage is not null)
                    await LocalizationService.SetCultureAsync(SelectedLanguage.CultureInfo);
            }
            else
            {
                await LocalizationService.SetCultureAsync(value.CultureInfo);
                IsRestartRequired = !_currentCulture.Equals(SelectedLanguage?.CultureInfo);
            }
        }
    }
}
