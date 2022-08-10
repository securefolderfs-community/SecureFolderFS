using System;
using System.Collections.Generic;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Services.UserPreferences;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.WinUI.ServiceImplementation.UserPreferences
{
    /// <inheritdoc cref="IVaultsSettingsService"/>
    internal sealed class VaultsSettingsService : MultipleFilesSettingsModel, IVaultsSettingsService
    {
        public VaultsSettingsService(IModifiableFolder? settingsFolder)
        {
            SettingsFolder = settingsFolder;
        }

        /// <inheritdoc/>
        protected override string? SettingsStorageName { get; } = Constants.LocalSettings.VAULTS_SETTINGS_FILENAME;

        // TODO: Important!!! Use individual files!!!
        // TODO: Also check other TODOs (with Important tag)
        [Obsolete("This property is obsolete. Use individual files to save separate contexts in separate files.")]
        public Dictionary<string, VaultContextDataModel>? VaultContexts
        {
            get => GetSetting<Dictionary<string, VaultContextDataModel>?>(() => null);
            set => SetSetting<Dictionary<string, VaultContextDataModel>?>(value);
        }

        [Obsolete("This property is obsolete. Use individual files to save separate contexts in separate files.")]
        public Dictionary<string, WidgetsContextDataModel>? WidgetContexts
        {
            get => GetSetting<Dictionary<string, WidgetsContextDataModel>?>(() => null);
            set => SetSetting<Dictionary<string, WidgetsContextDataModel>?>(value);
        }

        /// <inheritdoc/>
        public VaultContextDataModel GetVaultContextForId(string id)
        {
            VaultContexts ??= new();
            return VaultContexts.GetOrCreate(id, () => new());
        }

        /// <inheritdoc/>
        public WidgetsContextDataModel GetWidgetsContextForId(string id)
        {
            WidgetContexts ??= new();
            return WidgetContexts.GetOrCreate(id, () => new());
        }
    }
}
