using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Backend.Enums;
using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.Services;
using SecureFolderFS.Backend.Services.Settings;
using SecureFolderFS.Backend.ViewModels.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace SecureFolderFS.Backend.ViewModels.Pages.SettingsDialog
{
    public sealed class GeneralSettingsPageViewModel : BaseSettingsDialogPageViewModel
    {
        private ILocalizationService LocalizationService { get; } = Ioc.Default.GetRequiredService<ILocalizationService>();

        private IUpdateService UpdateService { get; } = Ioc.Default.GetRequiredService<IUpdateService>();

        private IApplicationSettingsService ApplicationSettingsService { get; } = Ioc.Default.GetRequiredService<IApplicationSettingsService>();

        public ObservableCollection<AppLanguageModel> AppLanguages { get; }

        public InfoBarViewModel VersionInfoBar { get; }

        private string? _UpdateStatusText;
        public string? UpdateStatusText
        {
            get => _UpdateStatusText;
            set => SetProperty(ref _UpdateStatusText, value);
        }

        private bool _IsUpdateSupported;
        public bool IsUpdateSupported
        {
            get => _IsUpdateSupported;
            set => SetProperty(ref _IsUpdateSupported, value);
        }

        private bool _IsRestartRequired;
        public bool IsRestartRequired
        {
            get => _IsRestartRequired;
            set => SetProperty(ref _IsRestartRequired, value);
        }

        private DateTime _UpdateLastChecked;
        public DateTime UpdateLastChecked
        {
            get => _UpdateLastChecked;
            set
            {
                if (SetProperty(ref _UpdateLastChecked, value) && ApplicationSettingsService.IsAvailable)
                {
                    ApplicationSettingsService.UpdateLastChecked = value;
                }
            }
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

        public IAsyncRelayCommand CheckForUpdatesCommand { get; }

        public GeneralSettingsPageViewModel()
        {
            if (ApplicationSettingsService.IsAvailable)
            {
                UpdateLastChecked = ApplicationSettingsService.UpdateLastChecked;
            }
            VersionInfoBar = new();
            AppLanguages = new(LocalizationService.GetLanguages());
            _UpdateStatusText = "Latest version installed";
            _IsUpdateSupported = false;

            CheckForUpdatesCommand = new AsyncRelayCommand(CheckForUpdates);            
        }

        public async Task ConfigureUpdates()
        {
            var updatingAppSupported = await UpdateService.AreAppUpdatesSupportedAsync(); 

            if (!updatingAppSupported)
            {
                IsUpdateSupported = false;
                VersionInfoBar.IsOpen = true;
                VersionInfoBar.MessageText = "Updates are not supported for sideloaded version.";
                VersionInfoBar.InfoBarSeverity = InfoBarSeverityType.Warning;
                VersionInfoBar.CanBeClosed = false;
            }
        }

        private async Task CheckForUpdates()
        {
            UpdateLastChecked = DateTime.Now;

            Debug.WriteLine(await UpdateService.IsNewUpdateAvailableAsync());
        }
    }
}
