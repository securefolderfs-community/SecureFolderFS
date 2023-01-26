using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.AvaloniaUI.Services;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.UI;
using SecureFolderFS.UI.Enums;

namespace SecureFolderFS.AvaloniaUI.ServiceImplementation.UserPreferences
{
    internal sealed class PlatformSettingsService : OnDeviceSettingsModel, IPlatformSettingsService
    {
        public PlatformSettingsService(IModifiableFolder settingsFolder)
            : base(settingsFolder)
        {
        }

        public ApplicationTheme Theme
        {
            get => GetSetting(() => ApplicationTheme.Default);
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            var settingsFile = await SettingsFolder.TryCreateFileAsync(Constants.LocalSettings.PLATFORM_SETTINGS_FILENAME, false, cancellationToken);
            if (settingsFile is null)
                return;

            SettingsDatabase = new SingleFileDatabaseModel(settingsFile, DoubleSerializedStreamSerializer.Instance);
            IsAvailable = true;
        }
    }
}