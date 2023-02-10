using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <summary>
    /// Represents a settings model where settings are stored on device in a folder.
    /// </summary>
    public abstract class LocalSettingsModel : SettingsModel, IAsyncInitialize
    {
        /// <summary>
        /// Gets the parent folder which stores the database.
        /// </summary>
        protected IModifiableFolder SettingsFolder { get; }

        protected LocalSettingsModel(IModifiableFolder settingsFolder)
        {
            SettingsFolder = settingsFolder;
        }

        /// <inheritdoc/>
        public abstract Task InitAsync(CancellationToken cancellationToken = default);
    }
}
