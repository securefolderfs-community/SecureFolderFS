using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls.Banners;
using SecureFolderFS.Shared;
using SecureFolderFS.Storage.Pickers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.ViewModels.Controls.Components;

namespace SecureFolderFS.Sdk.ViewModels.Views.Settings
{
    [Inject<ILocalizationService>, Inject<IApplicationService>, Inject<IFileExplorerService>]
    [Bindable(true)]
    public sealed partial class GeneralSettingsViewModel : BaseSettingsViewModel
    {
        private readonly CultureInfo _currentCulture;
        private bool _noNotify;

        [ObservableProperty] private ObservableCollection<LanguageViewModel> _Languages;
        [ObservableProperty] private LanguageViewModel? _SelectedLanguage;
        [ObservableProperty] private bool _IsRestartRequired;

        public UpdateBannerViewModel BannerViewModel { get; }

        public GeneralSettingsViewModel()
        {
            ServiceProvider = DI.Default;
            Title = "SettingsGeneral".ToLocalized();
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

        [RelayCommand]
        private async Task ExportSettingsAsync(CancellationToken cancellationToken)
        {
            await using var exportStream = await UserSettings.ExportAsync(cancellationToken);
            if (exportStream == System.IO.Stream.Null)
                return;

            var filter = new Dictionary<string, string>()
            {
                { Constants.Settings.EXPORTED_ARCHIVE_FILENAME, Constants.Settings.EXPORTED_ARCHIVE_EXTENSION }
            };
            await FileExplorerService.SaveFileAsync(Constants.Settings.EXPORTED_ARCHIVE_FILENAME, exportStream, filter, cancellationToken);
        }

        [RelayCommand]
        private async Task ImportSettingsAsync(CancellationToken cancellationToken)
        {
            var pickedFile = await FileExplorerService.PickFileAsync(new NameFilter([ Constants.Settings.EXPORTED_ARCHIVE_EXTENSION ]), false, cancellationToken);
            if (pickedFile is null)
                return;

            await using var fileStream = await pickedFile.OpenReadAsync(cancellationToken);
            var success = await UserSettings.ImportAsync(fileStream, cancellationToken);
            if (!success)
                return;

            // Settings were imported successfully, a restart is recommended
            IsRestartRequired = true;
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
