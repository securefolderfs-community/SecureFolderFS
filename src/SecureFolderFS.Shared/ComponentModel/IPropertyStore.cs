using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.ComponentModel
{
    /// <summary>
    /// Represents a property store to store data identified by <typeparamref name="TKey"/>.
    /// </summary>
    /// <typeparam name="TKey">The key to identify data with.</typeparam>
    public interface IPropertyStore<in TKey>
    {
        /// <summary>
        /// Gets a value from the property store.
        /// </summary>
        /// <typeparam name="TValue">The type of value.</typeparam>
        /// <param name="key">The name of the value to get.</param>
        /// <param name="defaultValue">Retrieves the default value. If <paramref name="defaultValue"/> is null, returns the default value.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. A value from the property store. The value is determined by the availability in the storage or by the <paramref name="defaultValue"/>.</returns>
        Task<TValue?> GetValueAsync<TValue>(TKey key, Func<TValue?>? defaultValue = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets a value in the property store.
        /// </summary>
        /// <typeparam name="TValue">The type of value.</typeparam>
        /// <param name="key">The name of the value to set.</param>
        /// <param name="value">The value to be stored.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If the value has been updated in the property store, returns true otherwise false.</returns>
        Task<bool> SetValueAsync<TValue>(TKey key, TValue? value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a value associated with the specified key from the property store asynchronously.
        /// </summary>
        /// <param name="key">The key associated with the value to remove.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Returns true if the value was successfully removed; otherwise, false.</returns>
        Task<bool> RemoveAsync(TKey key, CancellationToken cancellationToken = default);
    }
}