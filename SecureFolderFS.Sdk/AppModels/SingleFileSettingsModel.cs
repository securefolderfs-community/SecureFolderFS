using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Storage.Enums;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="ISettingsModel"/>
    public abstract class SingleFileSettingsModel : SettingsModel
    {
        /// <summary>
        /// Gets the name of the settings store.
        /// </summary>
        protected abstract string? SettingsStorageName { get; }

        /// <inheritdoc/>
        public override async Task<bool> LoadSettingsAsync(CancellationToken cancellationToken = default)
        {
            if  (SettingsFolder is null || SettingsDatabase is not ISingleFileSerializationModel serializationModel || string.IsNullOrEmpty(SettingsStorageName))
                return false;

            var settingsFile = await SettingsFolder.CreateFileAsync(SettingsStorageName, CreationCollisionOption.OpenIfExists, cancellationToken);
            if (settingsFile is null)
                return false;

            return IsAvailable = await serializationModel.LoadAsync(settingsFile, cancellationToken);
        }

        /// <inheritdoc/>
        public override async Task<bool> SaveSettingsAsync(CancellationToken cancellationToken = default)
        {
            if (SettingsFolder is null || SettingsDatabase is not ISingleFileSerializationModel serializationModel || string.IsNullOrEmpty(SettingsStorageName))
                return false;

            var settingsFile = await SettingsFolder.CreateFileAsync(SettingsStorageName, CreationCollisionOption.OpenIfExists, cancellationToken);
            if (settingsFile is null)
                return false;

            return await serializationModel.SaveAsync(settingsFile, cancellationToken);
        }
    }
}
