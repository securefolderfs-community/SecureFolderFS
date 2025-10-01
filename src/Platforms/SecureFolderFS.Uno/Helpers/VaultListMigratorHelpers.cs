using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.UI;

namespace SecureFolderFS.Uno.Helpers
{
    internal static class VaultListMigratorHelpers
    {
        public static async Task<IFile?> TryGetVaultsFileAsync(CancellationToken cancellationToken)
        {
            try
            {
                var settingsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), Constants.FileNames.SETTINGS_FOLDER_NAME);
                var settingsFolder = new SystemFolder(new DirectoryInfo(settingsFolderPath));

                return await settingsFolder.GetFileByNameAsync(Constants.FileNames.SAVED_VAULTS_FILENAME, cancellationToken);
            }
            catch (Exception)
            {
                return null;
            }
        }
        
        public static bool IsMigrated()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            return (localSettings.Values["vaults_migrated_one"] as bool?) ?? false;
        }
        
        public static void SetMigrated(bool value = true)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["vaults_migrated_one"] = value;
        }
        
        public static async Task TryMigrateVaultsAsync(IFile settingsFile, IAsyncSerializer<Stream> serializer, CancellationToken cancellationToken)
        {
            try
            {
                await using var stream = await settingsFile.OpenReadAsync(cancellationToken);
                var deserialized = await serializer.TryDeserializeAsync<Stream, OldVaultListDataModel?>(stream, cancellationToken);

                if (deserialized is not { SavedVaults: { } savedVaults })
                    return;

                if (savedVaults.IsEmpty())
                    return;

                var newDataModel = new NewVaultListDataModel()
                {
                    PersistedVaults = []
                };
                foreach (var item in savedVaults)
                    newDataModel.PersistedVaults.Add(new VaultDataModel(item.PersistableId, item.VaultName, item.LastAccessDate, new LocalStorageSourceDataModel()));

                await using var serialized = await serializer.TrySerializeAsync(newDataModel, cancellationToken);
                if (serialized is null)
                    return;

                stream.Position = 0L;
                await serialized.CopyToAsync(stream, cancellationToken);
            }
            catch (Exception ex)
            {
                _ = ex;
            }
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    file class NewVaultListDataModel
    {
        public List<VaultDataModel>? PersistedVaults { get; set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    file class OldVaultListDataModel
    {
        public List<OldVaultDataModel>? SavedVaults { get; set; }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    file class OldVaultDataModel(string? PersistableId, string? VaultName, DateTime? LastAccessDate)
    {
        [JsonPropertyName("Id")]
        public string? PersistableId { get; set; } = PersistableId;

        [JsonPropertyName("Name")]
        public string? VaultName { get; set; } = VaultName;

        [JsonPropertyName("LastAccessDate")]
        public DateTime? LastAccessDate { get; set; } = LastAccessDate;
    }
}
