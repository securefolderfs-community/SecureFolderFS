using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Services.UserPreferences;
using SecureFolderFS.Sdk.Storage.Enums;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.WinUI.ServiceImplementation.UserPreferences
{
    /// <inheritdoc cref="IVaultsWidgetsService"/>
    internal sealed class VaultsWidgetsService : OnDeviceSettingsModel, IVaultsWidgetsService
    {
        public VaultsWidgetsService(IModifiableFolder settingsFolder)
            : base(settingsFolder)
        {
        }

        /// <inheritdoc/>
        public WidgetsContextDataModel GetWidgetsContextForId(string id)
        {
            var widgetsContext = GetSetting<WidgetsContextDataModel>(() => new(), id);
            return widgetsContext!; // TODO: Ensure not null
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            var folderOfSettings = await SettingsFolder.TryCreateFolderAsync(Constants.LocalSettings.VAULTS_WIDGETS_FOLDERNAME, CreationCollisionOption.OpenIfExists, cancellationToken);
            if (folderOfSettings is not IModifiableFolder modifiableFolderOfSettings)
                return;

            SettingsDatabase = new MultipleFilesDatabaseModel(modifiableFolderOfSettings, StreamSerializer.Instance);
            IsAvailable = true;
        }
    }
}
