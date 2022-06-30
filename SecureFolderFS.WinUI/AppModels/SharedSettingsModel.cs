using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.WinUI.AppModels
{
    /// <summary>
    /// Manages a settings node of shared settings.
    /// </summary>
    internal abstract class SharedSettingsModel : SingleFileSettingsModel
    {
        protected readonly ISettingsModel originSettingsModel;

        protected SharedSettingsModel(ISettingsModel originSettingsModel)
        {
            this.originSettingsModel = originSettingsModel;
        }

        /// <inheritdoc/>
        public override bool IsAvailable => originSettingsModel.IsAvailable;

        /// <inheritdoc/>
        protected sealed override string? SettingsStorageName { get; } = null;

        /// <inheritdoc/>
        public override Task<bool> LoadSettingsAsync(CancellationToken cancellationToken = default)
        {
            return originSettingsModel.LoadSettingsAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public override Task<bool> SaveSettingsAsync(CancellationToken cancellationToken = default)
        {
            return originSettingsModel.SaveSettingsAsync(cancellationToken);
        }
    }
}
