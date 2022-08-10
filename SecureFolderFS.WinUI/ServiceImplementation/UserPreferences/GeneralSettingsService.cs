using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.UserPreferences;
using SecureFolderFS.WinUI.AppModels;
using System;

namespace SecureFolderFS.WinUI.ServiceImplementation.UserPreferences
{
    /// <inheritdoc cref="IGeneralSettingsService"/>
    internal sealed class GeneralSettingsService : SharedSettingsModel, IGeneralSettingsService
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
