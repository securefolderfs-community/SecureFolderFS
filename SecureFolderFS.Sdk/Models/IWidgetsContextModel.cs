using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Represents a context that holds widgets settings and layout of an individual vault.
    /// </summary>
    public interface IWidgetsContextModel
    {
        /// <summary>
        /// Gets the vault model that is associated with this context.
        /// </summary>
        IVaultModel VaultModel { get; }

        /// <summary>
        /// Gets persisted or new widget model identified by <paramref name="widgetId"/>.
        /// </summary>
        /// <param name="widgetId">The id of a widget.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, the value is <see cref="IWidgetModel"/>, otherwise null.</returns>
        Task<IWidgetModel?> GetWidgetAsync(string widgetId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes persisted widget model identified by <paramref name="widgetId"/>.
        /// </summary>
        /// <param name="widgetId">The id of a widget.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns true, otherwise false.</returns>
        Task<bool> RemoveWidgetAsync(string widgetId, CancellationToken cancellationToken = default);
    }
}
