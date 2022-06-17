using System.Collections.Generic;
using System.Linq;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.WinUI.Storage;

namespace SecureFolderFS.WinUI.ServiceImplementation.Settings
{
    /// <summary>
    /// TODO
    /// </summary>
    internal sealed partial class SettingsService : SingleFileSettingsModel, ISettingsService
    {
        public SettingsService(IFolder settingsFolder, IFileSystemService fileSystemService)
        {
            FilePool = new CachingFilePool(settingsFolder, fileSystemService);
            SettingsDatabase = new DictionarySettingsDatabaseModel(new JsonToStreamSerializer());
        }

        /// <inheritdoc/>
        protected override string? SettingsStorageName { get; } = Constants.LocalSettings.USER_SETTINGS_FILE_NAME;

        /// <inheritdoc/>
        public Dictionary<VaultIdModel, VaultViewModel> SavedVaults
        {
            get => GetSetting<List<KeyValuePair<VaultIdModel, VaultViewModel>>>(() => new())!.ToDictionary()!;
            set => SetSetting<List<KeyValuePair<VaultIdModel, VaultViewModel>>>(value.ToList());
        }

        internal ISettingsDatabaseModel GetDatabaseModel()
        {
            return SettingsDatabase!;
        }
    }

    /// <inheritdoc cref="SettingsService"/>
    internal sealed partial class SettingsService// : IGeneralSettingsService
    {

    }
}
