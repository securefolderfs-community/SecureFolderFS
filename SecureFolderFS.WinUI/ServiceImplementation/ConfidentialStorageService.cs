using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Storage;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.Services;
using SecureFolderFS.WinUI.Serialization;
using SecureFolderFS.WinUI.Serialization.Implementation;

#nullable enable

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    internal sealed class ConfidentialStorageService : BaseJsonSettings, IConfidentialStorageService
    {
        public ConfidentialStorageService()
        {
            SettingsSerializer = new DPAPISettingsSerializer();
            JsonSettingsSerializer = new DefaultJsonSettingsSerializer();
            JsonSettingsDatabase = new CachingJsonSettingsDatabase(SettingsSerializer, JsonSettingsSerializer);

            Initialize(Path.Combine(ApplicationData.Current.LocalFolder.Path, Constants.LocalSettings.SETTINGS_FOLDER_NAME, Constants.LocalSettings.CONFIDENTIAL_SETTINGS_FILE_NAME));
        }

        public Dictionary<VaultIdModel, VaultModel> SavedVaultModels
        {
            get => Get<List<KeyValuePair<VaultIdModel, VaultModel>>>(() => new())!.ToDictionary()!;
            set => Set<List<KeyValuePair<VaultIdModel, VaultModel>>>(value.ToList());
        }
    }
}
