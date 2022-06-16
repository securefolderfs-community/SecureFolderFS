using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Storage.StoragePool;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="ISettingsModel"/>
    public abstract class SettingsModel : ISettingsModel
    {
        // TODO: Add `required` modifier
        protected IFilePool FilePool { get; init; }
        protected ISettingsDatabaseModel SettingsDatabase { get; init; }

        /// <inheritdoc/>
        public bool IsAvailable { get; protected set; }

        /// <summary>
        /// Gets the name of the settings store.
        /// </summary>
        protected abstract string SettingsStorageName { get; }

        /// <summary>
        /// Gets a value of a setting defined by <paramref name="settingName"/>.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="defaultValue">Retrieves the default value. If <paramref name="defaultValue"/> is null, returns the default value of <typeparamref name="T"/>.</param>
        /// <param name="settingName">The name of the setting.</param>
        /// <returns>A requested setting. The value is determined by the availability of the setting in the storage or by the <paramref name="defaultValue"/>.</returns>
        protected virtual T? GetSetting<T>(Func<T?>? defaultValue, [CallerMemberName] string settingName = "")
        {
            if (string.IsNullOrEmpty(settingName))
                return defaultValue is not null ? defaultValue() : default;

            return SettingsDatabase.GetValue(settingName, defaultValue);
        }

        /// <summary>
        /// Sets a setting value defined by <paramref name="settingName"/>.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="value">The value to be stored.</param>
        /// <param name="settingName">The name of the setting.</param>
        /// <returns>If the setting has been updated, returns true otherwise false.</returns>
        protected virtual bool SetSetting<T>(T? value, [CallerMemberName] string settingName = "")
        {
            if (string.IsNullOrEmpty(settingName))
                return false;

            return SettingsDatabase.SetValue(settingName, value);
        }

        /// <inheritdoc/>
        public virtual async Task<bool> LoadSettingsAsync(CancellationToken cancellationToken = default)
        {
            var settingsFile = await FilePool.RequestFileAsync(SettingsStorageName, cancellationToken);
            if (settingsFile is null)
                return false;

            return await SettingsDatabase.LoadFromFile(settingsFile, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<bool> SaveSettingsAsync(CancellationToken cancellationToken = default)
        {
            var settingsFile = await FilePool.RequestFileAsync(SettingsStorageName, cancellationToken);
            if (settingsFile is null)
                return false;

            return await SettingsDatabase.SaveToFile(settingsFile, cancellationToken);
        }
    }
}
