using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Settings.Banners;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels.Pages.Settings
{
    public sealed class GeneralSettingsPageViewModel : ObservableObject, IAsyncInitialize
    {
        public ILocalizationService LocalizationService { get; } = Ioc.Default.GetRequiredService<ILocalizationService>();

        public UpdateBannerViewModel BannerViewModel { get; }

        public ObservableCollection<LanguageViewModel> Languages { get; }

        public GeneralSettingsPageViewModel()
        {
            BannerViewModel = new();
            Languages = new();
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            foreach (var item in LocalizationService.AppLanguages)
                Languages.Add(new(item));

            return Task.CompletedTask;
        }
    }
}
