using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.UserPreferences;
using SecureFolderFS.Sdk.AppModels;

namespace SecureFolderFS.UI.ServiceImplementation.UserPreferences
{
    /// <inheritdoc cref="IGeneralSettingsService"/>
    public sealed class GeneralSettingsService : SharedSettingsModel, IGeneralSettingsService
    {
        public GeneralSettingsService(IDatabaseModel<string> originSettingsDatabase, ISettingsModel originSettingsModel)
            : base(originSettingsDatabase, originSettingsModel)
        {
        }

        /// <inheritdoc/>
        public DateTime UpdateLastChecked
        {
            get => GetSetting<DateTime>(() => new());
            set => SetSetting<DateTime>(value);
        }
    }
}
