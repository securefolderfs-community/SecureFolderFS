using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Storage.Enums;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="ISettingsModel"/>
    public abstract class SingleFileSettingsModel : SettingsModel
    {
        private readonly IAsyncSerializer<Stream> _serializer;

        // TODO: This is temporary until InitializeSettingsAsync() is removed
        protected SingleFileSettingsModel(IAsyncSerializer<Stream>? serializer = null)
        {
            _serializer = serializer ?? JsonToStreamSerializer.Instance;
        }

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

        // TODO: Initialize the database earlier, not there. And remove it from other model too
        private async Task<bool> InitializeSettingsAsync(CancellationToken cancellationToken)
        {
            if (SettingsFolder is null || string.IsNullOrEmpty(SettingsStorageName))
                return false;

            if (SettingsDatabase is null)
            {
                var settingsFile = await SettingsFolder.TryCreateFileAsync(SettingsStorageName, CreationCollisionOption.OpenIfExists, cancellationToken);
                if (settingsFile is null)
                    return false;

                SettingsDatabase = new SingleFileDatabaseModel(settingsFile, _serializer);
                IsAvailable = true;
            }

            return true;
        }
    }
}
