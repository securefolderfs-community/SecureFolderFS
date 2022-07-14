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
    }
}
