using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.SettingsPersistence;
using SecureFolderFS.Shared.Utils;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Dialogs
{
    public sealed partial class ChangelogDialogViewModel : DialogViewModel, IAsyncInitialize
    {
        private readonly bool _usePreviousVersion;

        [ObservableProperty] private string? _UpdateText;

        private IApplicationService ApplicationService { get; } = Ioc.Default.GetRequiredService<IApplicationService>();

        private IChangelogService ChangelogService { get; } = Ioc.Default.GetRequiredService<IChangelogService>();

        private IAppSettings AppSettings { get; } = Ioc.Default.GetRequiredService<ISettingsService>().AppSettings;

        public ChangelogDialogViewModel(bool usePreviousVersion = false)
        {
            _usePreviousVersion = usePreviousVersion;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            var changelogBuilder = new StringBuilder();
            var loadLatest = !_usePreviousVersion || (_usePreviousVersion && string.IsNullOrEmpty(AppSettings.LastVersion));

            if (loadLatest)
            {
                var appVersion = ApplicationService.GetAppVersion();
                var changelog = await ChangelogService.GetChangelogAsync(appVersion, cancellationToken);
                if (changelog is null)
                {
                    UpdateText = "No update info available";
                    return;
                }

                BuildChangelog(changelog, changelogBuilder);
            }
            else
            {
                var previousVersion = new AppVersion(new(AppSettings.LastVersion!), ApplicationService.Platform);
                await foreach (var item in ChangelogService.GetChangelogSinceAsync(previousVersion, cancellationToken))
                {
                    BuildChangelog(item, changelogBuilder);
                }
            }

            UpdateText = changelogBuilder.ToString();
        }

        // TODO: Perhaps use an IFormatProvider somehow?
        private static void BuildChangelog(ChangelogDataModel changelog, StringBuilder changelogBuilder)
        {
            changelogBuilder.Append(changelog.Name);
            changelogBuilder.Append("\n---\n");
            changelogBuilder.Append(changelog.Description.Replace("\r\n", "\r\n\n"));
            changelogBuilder.Append("\n\n");
        }
    }
}
