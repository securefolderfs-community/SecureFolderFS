using SecureFolderFS.Sdk.ViewModels;

namespace SecureFolderFS.Sdk.Contexts
{
    /// <summary>
    /// A context for interacting with <see cref="Sdk.ViewModels.UnlockedVaultViewModel"/>.
    /// </summary>
    public interface IUnlockedViewContext : IVaultViewContext
    {
        /// <summary>
        /// Gets the <see cref="Sdk.ViewModels.UnlockedVaultViewModel"/> associated with this context.
        /// </summary>
        UnlockedVaultViewModel UnlockedVaultViewModel { get; }
    }
}
