using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Represents a database where settings are contained.
    /// </summary>
    public interface ISettingsDatabaseModel
    {
        /// <summary>
        /// Gets a value from the database.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="key">The name of the value to get.</param>
        /// <param name="defaultValue">Retrieves the default value. If <paramref name="defaultValue"/> is null, returns the default value of <typeparamref name="T"/>.</param>
        /// <returns>A value from the database. The value is determined by the availability of the setting in the storage or by the <paramref name="defaultValue"/>.</returns>
        T? GetValue<T>(string key, Func<T?>? defaultValue);

        /// <summary>
        /// Sets a value in the database.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="key">The name of the value to set.</param>
        /// <param name="value">The value to be stored.</param>
        /// <returns>If the value has been updated in the database, returns true otherwise false.</returns>
        bool SetValue<T>(string key, T? value);

        /// <summary>
        /// Loads the database from <paramref name="file"/>.
        /// </summary>
        /// <param name="file">The file to load the database from.</param>
        /// <param name="cancellationToken">Cancellation token of the action.</param>
        /// <returns>If database was successfully loaded, returns true otherwise false.</returns>
        Task<bool> LoadFromFile(IFile file, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves the database to <paramref name="file"/>.
        /// </summary>
        /// <param name="file">The database file.</param>
        /// <param name="cancellationToken">Cancellation token of the action.</param>
        /// <returns>If database was successfully saved, returns true otherwise false.</returns>
        Task<bool> SaveToFile(IFile file, CancellationToken cancellationToken = default);
    }
}
