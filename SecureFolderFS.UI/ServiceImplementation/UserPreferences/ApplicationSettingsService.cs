using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Services.UserPreferences;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;

namespace SecureFolderFS.UI.ServiceImplementation.UserPreferences
{
    /// <inheritdoc cref="IApplicationSettingsService"/>
    public sealed class ApplicationSettingsService : OnDeviceSettingsModel, IApplicationSettingsService
    {
        public ApplicationSettingsService(IModifiableFolder settingsFolder)
            : base(settingsFolder)
        {
        }

        /// <inheritdoc/>
        public bool IsIntroduced
        {
            get => GetSetting(() => false);
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public string? LastVaultFolderId
        {
            get => GetSetting<string>();
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            var settingsFile = await SettingsFolder.TryCreateFileAsync(Constants.LocalSettings.APPLICATION_SETTINGS_FILENAME, false, cancellationToken);
            if (settingsFile is null)
                return;

            SettingsDatabase = new SingleFileDatabaseModel(settingsFile, DoubleSerializedStreamSerializer.Instance);
            IsAvailable = true;
        }
    }
}
