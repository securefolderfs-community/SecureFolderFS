using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Storage;
using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.Services;
using SecureFolderFS.Backend.ViewModels;
using SecureFolderFS.WinUI.Serialization;
using SecureFolderFS.WinUI.Serialization.Implementation;
using SecureFolderFS.Backend.Extensions;

#nullable enable

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    internal sealed class SettingsService : BaseJsonSettings, ISettingsService
    {
        public SettingsService()
        {
            SettingsSerializer = new DefaultSettingsSerializer();
            JsonSettingsSerializer = new DefaultJsonSettingsSerializer();
            JsonSettingsDatabase = new CachingJsonSettingsDatabase(SettingsSerializer, JsonSettingsSerializer);

            Initialize(Path.Combine(ApplicationData.Current.LocalFolder.Path, Constants.LocalSettings.SETTINGS_FOLDER_NAME, Constants.LocalSettings.USER_SETTINGS_FILE_NAME));
        }

        public Dictionary<VaultIdModel, VaultViewModel> SavedVaults
        {
            get => Get<List<KeyValuePair<VaultIdModel, VaultViewModel>>>(new())!.ToDictionary()!;
            set => Set<List<KeyValuePair<VaultIdModel, VaultViewModel>>>(value.ToList());
        }
    }
}
