using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.VaultPersistence;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using System.Collections.Generic;

namespace SecureFolderFS.UI.ServiceImplementation.VaultPersistence
{
    /// <inheritdoc cref="IVaultConfigurations"/>
    public sealed class VaultConfigurations : SettingsModel, IVaultConfigurations
    {
        /// <inheritdoc/>
        protected override IDatabaseModel<string> SettingsDatabase { get; }

        public VaultConfigurations(IModifiableFolder settingsFolder)
        {
            SettingsDatabase = new SingleFileDatabaseModel(Constants.LocalSettings.SAVED_VAULTS_FILENAME, settingsFolder, DoubleSerializedStreamSerializer.Instance);
        }

        /// <inheritdoc/>
        public ICollection<VaultDataModel>? SavedVaults
        {
            get => GetSetting<List<VaultDataModel>?>();
            set => SetSetting(value);
        }
    }
}
