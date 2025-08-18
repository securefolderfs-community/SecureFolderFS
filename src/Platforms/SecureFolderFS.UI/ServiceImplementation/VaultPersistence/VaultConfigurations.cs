using System.Collections.Generic;
using System.Text.Json;
using OwlCore.Storage;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.AppModels.Database;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.VaultPersistence;
using SecureFolderFS.UI.AppModels;

namespace SecureFolderFS.UI.ServiceImplementation.VaultPersistence
{
    /// <inheritdoc cref="IVaultConfigurations"/>
    public sealed class VaultConfigurations : SettingsModel, IVaultConfigurations
    {
        /// <inheritdoc/>
        protected override IDatabaseModel<string> SettingsDatabase { get; }

        public VaultConfigurations(IModifiableFolder settingsFolder)
        {
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            options.Converters.Add(new VaultDataSourceJsonConverter());

            SettingsDatabase = new SingleFileDatabaseModel(Constants.FileNames.SAVED_VAULTS_FILENAME, settingsFolder, new DoubleSerializedStreamSerializer(options));
        }

        /// <inheritdoc/>
        public IList<VaultDataModel>? PersistedVaults
        {
            get => GetSetting<List<VaultDataModel>?>();
            set => SetSetting(value);
        }
    }
}
