using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Storage.Enums;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="ISettingsModel"/>
    public abstract class MultipleFilesSettingsModel : SettingsModel
    {
        /// <inheritdoc/>
        public override async Task<bool> LoadSettingsAsync(CancellationToken cancellationToken = default)
        {
            if (!await InitializeSettingsAsync(cancellationToken))
                return false;

            return await base.LoadSettingsAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public override async Task<bool> SaveSettingsAsync(CancellationToken cancellationToken = default)
        {
            if (!await InitializeSettingsAsync(cancellationToken))
                return false;

            return await base.SaveSettingsAsync(cancellationToken);
        }

        private async Task<bool> InitializeSettingsAsync(CancellationToken cancellationToken)
        {
            if (SettingsFolder is null || string.IsNullOrEmpty(SettingsStorageName))
                return false;

            if (SettingsDatabase is null)
            {
                var settingsFolder = await SettingsFolder.TryCreateFolderAsync(SettingsStorageName, CreationCollisionOption.OpenIfExists, cancellationToken);
                if (settingsFolder is not IModifiableFolder modifiableFolder)
                    return false;

                SettingsDatabase = new MultipleFilesDatabaseModel(modifiableFolder, JsonToStreamSerializer.Instance);
                IsAvailable = true;
            }

            return true;
        }
    }
}
