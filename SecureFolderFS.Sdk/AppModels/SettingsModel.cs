using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Storage.StoragePool;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="ISettingsModel"/>
    public abstract class SettingsModel : ISettingsModel
    {
        protected readonly IAsyncSerializer<Stream> serializer;
        protected readonly IFilePool filePool;

        /// <inheritdoc/>
        public bool IsAvailable { get; protected set; }

        protected SettingsModel(IFilePool filePool, IAsyncSerializer<Stream> serializer)
        {
            this.filePool = filePool;
            this.serializer = serializer;
        }

        /// <summary>
        /// Gets a value of a setting defined by <paramref name="settingName"/>.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="defaultValue">Retrieves the default value of a setting.</param>
        /// <param name="settingName">The name of the setting.</param>
        /// <returns>A requested setting. The value is determined by the availability of the setting in the storage or by the <paramref name="defaultValue"/>.</returns>
        protected T GetSetting<T>(Func<T>? defaultValue, [CallerMemberName] string settingName = "")
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets a setting value defined by <paramref name="settingName"/>.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="value">The value to be stored.</param>
        /// <param name="settingName">The name of the setting.</param>
        /// <returns>If the setting has been updated returns true, otherwise false.</returns>
        protected bool SetSetting<T>(T value, [CallerMemberName] string settingName = "")
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual Task<bool> SaveSettingsAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc/>
        public virtual Task<bool> LoadSettingsAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
