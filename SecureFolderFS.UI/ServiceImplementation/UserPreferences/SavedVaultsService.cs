using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Services.UserPreferences;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;

namespace SecureFolderFS.UI.ServiceImplementation.UserPreferences
{
    /// <inheritdoc cref="ISavedVaultsService"/>
    public sealed class SavedVaultsService : OnDeviceSettingsModel, ISavedVaultsService
    {
        public SavedVaultsService(IModifiableFolder settingsFolder)
            : base(settingsFolder)
        {
        }

        /// <inheritdoc/>
        public List<string>? VaultPaths
        {
            get => GetSetting<List<string>?>();
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            var settingsFile = await SettingsFolder.TryCreateFileAsync(Constants.LocalSettings.SAVED_VAULTS_FILENAME, false, cancellationToken);
            if (settingsFile is null)
                return;

            SettingsDatabase = new SingleFileDatabaseModel(settingsFile, DoubleSerializedStreamSerializer.Instance);
            IsAvailable = true;
        }
    }
}
