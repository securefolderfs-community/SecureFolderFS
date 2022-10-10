using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <summary>
    /// Represents a settings model where settings are stored on device within a settings folder.
    /// </summary>
    public abstract class OnDeviceSettingsModel : SettingsModel, IAsyncInitialize
    {
        protected IModifiableFolder SettingsFolder { get; }

        protected OnDeviceSettingsModel(IModifiableFolder settingsFolder)
        {
            SettingsFolder = settingsFolder;
        }

        /// <inheritdoc/>
        public override async Task<bool> LoadSettingsAsync(CancellationToken cancellationToken = default)
        {
            if (!IsAvailable)
                await InitAsync(cancellationToken);

            return await base.LoadSettingsAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public override async Task<bool> SaveSettingsAsync(CancellationToken cancellationToken = default)
        {
            if (!IsAvailable)
                await InitAsync(cancellationToken);

            return await base.SaveSettingsAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public abstract Task InitAsync(CancellationToken cancellationToken = default);
    }
}
