using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.UserPreferences;
using SecureFolderFS.Sdk.Storage.Enums;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.WinUI.ServiceImplementation.UserPreferences
{
    /// <inheritdoc cref="ISettingsService"/>
    internal sealed class SettingsService : OnDeviceSettingsModel, ISettingsService
    {
        public SettingsService(IModifiableFolder settingsFolder)
            : base(settingsFolder)
        {
        }

        internal IDatabaseModel<string> GetDatabaseModel()
        {
            return SettingsDatabase!;
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            var settingsFile = await SettingsFolder.TryCreateFileAsync(Constants.LocalSettings.USER_SETTINGS_FILENAME, CreationCollisionOption.OpenIfExists, cancellationToken);
            if (settingsFile is null)
                return;

            SettingsDatabase = new SingleFileDatabaseModel(settingsFile, StreamSerializer.Instance);
            IsAvailable = true;
        }
    }
}
