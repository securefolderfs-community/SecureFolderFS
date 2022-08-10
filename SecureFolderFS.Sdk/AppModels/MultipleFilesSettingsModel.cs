using System;
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
        }

        /// <inheritdoc/>
        public override Task<bool> SaveSettingsAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        private async Task<bool> InitializeSettingsAsync(CancellationToken cancellationToken)
        {
            if (SettingsFolder is null || string.IsNullOrEmpty(SettingsStorageName))
                return false;

            if (SettingsDatabase is not null)
            {
                var settingsFile = await SettingsFolder.TryCreateFileAsync(SettingsStorageName, CreationCollisionOption.OpenIfExists, cancellationToken);
                if (settingsFile is null)
                    return false;

                SettingsDatabase = new SingleFileDatabaseModel(settingsFile, JsonToStreamSerializer.Instance);
            }

            return true;
        }
    }
}
