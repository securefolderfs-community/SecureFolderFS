using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <summary>
    /// Manages a settings node of shared settings.
    /// </summary>
    public abstract class SharedSettingsModel : SettingsModel
    {
        protected ISettingsModel OriginSettingsModel { get; }

        protected SharedSettingsModel(IDatabaseModel<string> originSettingsDatabase, ISettingsModel originSettingsModel)
        {
            SettingsDatabase = originSettingsDatabase;
            OriginSettingsModel = originSettingsModel;
        }

        /// <inheritdoc/>
        public override bool IsAvailable => OriginSettingsModel.IsAvailable;

        /// <inheritdoc/>
        public sealed override Task<bool> LoadSettingsAsync(CancellationToken cancellationToken = default)
        {
            return OriginSettingsModel.LoadSettingsAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public sealed override Task<bool> SaveSettingsAsync(CancellationToken cancellationToken = default)
        {
            return OriginSettingsModel.SaveSettingsAsync(cancellationToken);
        }
    }
}
