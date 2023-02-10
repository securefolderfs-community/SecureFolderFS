using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;

namespace SecureFolderFS.UI.ServiceImplementation.Settings
{
    /// <inheritdoc cref="IApplicationSettings"/>
    public sealed class ApplicationSettings : LocalSettingsModel, IApplicationSettings
    {
        /// <inheritdoc/>
        protected override IDatabaseModel<string> SettingsDatabase { get; }

        public ApplicationSettings(IModifiableFolder settingsFolder)
            : base(settingsFolder)
        {
            SettingsDatabase = new SingleFileDatabaseModel()
        }

        /// <inheritdoc/>
        public DateTime UpdateLastChecked
        {
            get => GetSetting<DateTime>(() => new());
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public string? LastVaultFolderId
        {
            get => GetSetting<string>();
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public bool IsIntroduced
        {
            get => GetSetting(() => false);
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            var settingsFile = await SettingsFolder.TryCreateFileAsync(Constants.LocalSettings.APPLICATION_SETTINGS_FILENAME, false, cancellationToken);
            if (settingsFile is null)
                return;

            SettingsDatabase = new SingleFileDatabaseModel(settingsFile, DoubleSerializedStreamSerializer.Instance);

            await LoadAsync(cancellationToken);
        }
    }
}
