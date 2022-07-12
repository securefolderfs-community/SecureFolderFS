using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Represents a model for unlocking secure stores.
    /// </summary>
    /// <typeparam name="TUnlocked">The type to return when unlocked.</typeparam>
    public interface IUnlockingModel<TUnlocked> : IAsyncInitialize, IDisposable
        where TUnlocked : class
    {
        /// <summary>
        /// Checks whether the <see cref="IUnlockingModel{TUnlocked}"/> supports unlocking of this store.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The value is true if unlocking is supported, otherwise false.</returns>
        Task<bool> IsSupportedAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Unlocks and returns <typeparamref name="TUnlocked"/>.
        /// </summary>
        /// <param name="password">The password used for unlocking.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, value is <typeparamref name="TUnlocked"/>, otherwise null.</returns>
        Task<TUnlocked?> UnlockAsync(IPassword password, CancellationToken cancellationToken = default);
    }
}
