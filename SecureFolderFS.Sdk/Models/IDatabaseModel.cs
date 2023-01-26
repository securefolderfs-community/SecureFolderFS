using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Represents a database to store data identified by <typeparamref name="TKey"/>.
    /// </summary>
    /// <typeparam name="TKey">The key to identify data with.</typeparam>
    public interface IDatabaseModel<in TKey>
    {
        /// <summary>
        /// Gets a value from the database.
        /// </summary>
        /// <typeparam name="TValue">The type of value.</typeparam>
        /// <param name="key">The name of the value to get.</param>
        /// <param name="defaultValue">Retrieves the default value. If <paramref name="defaultValue"/> is null, returns the default value of <typeparamref name="TValue"/>.</param>
        /// <returns>A value from the database. The value is determined by the availability in the storage or by the <paramref name="defaultValue"/>.</returns>
        TValue? GetValue<TValue>(TKey key, Func<TValue?>? defaultValue = null);

        /// <summary>
        /// Sets a value in the database.
        /// </summary>
        /// <typeparam name="TValue">The type of value.</typeparam>
        /// <param name="key">The name of the value to set.</param>
        /// <param name="value">The value to be stored.</param>
        /// <returns>If the value has been updated in the database, returns true otherwise false.</returns>
        bool SetValue<TValue>(TKey key, TValue? value);

        /// <summary>
        /// Loads data into the database.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful returns true, otherwise false.</returns>
        Task<bool> LoadAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves all persisted contents of the database.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful returns true, otherwise false.</returns>
        Task<bool> SaveAsync(CancellationToken cancellationToken = default);
    }
}
