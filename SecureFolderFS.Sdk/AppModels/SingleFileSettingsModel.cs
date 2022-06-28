using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Models;

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
            if (FilePool is null || SettingsDatabase is null || string.IsNullOrEmpty(SettingsStorageName))
                return false;

            var settingsFile = await FilePool.RequestFileAsync(SettingsStorageName, cancellationToken).ConfigureAwait(false);
            if (settingsFile is null)
                return false;

            return await SettingsDatabase.LoadFromFile(settingsFile, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public override async Task<bool> SaveSettingsAsync(CancellationToken cancellationToken = default)
        {
            if (FilePool is null || SettingsDatabase is null || string.IsNullOrEmpty(SettingsStorageName))
                return false;

            var settingsFile = await FilePool.RequestFileAsync(SettingsStorageName, cancellationToken).ConfigureAwait(false);
            if (settingsFile is null)
                return false;

            return await SettingsDatabase.SaveToFile(settingsFile, cancellationToken).ConfigureAwait(false);
        }
    }
}
