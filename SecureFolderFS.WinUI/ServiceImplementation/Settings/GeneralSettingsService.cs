using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.WinUI.AppModels;

namespace SecureFolderFS.WinUI.ServiceImplementation.Settings
{
    /// <inheritdoc cref="IGeneralSettingsService"/>
    internal sealed class GeneralSettingsService : SharedSettingsModel, IGeneralSettingsService
    {
        public GeneralSettingsService(ISettingsDatabaseModel originSettingsDatabase, ISettingsModel originSettingsModel)
            : base(originSettingsModel)
        {
            SettingsDatabase = originSettingsDatabase;
        }
    }
}
