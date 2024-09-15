namespace SecureFolderFS.Sdk.Enums
{
    /// <summary>
    /// Determines how the vault login view should be handled.
    /// </summary>
    public enum LoginViewType
    {
        /// <summary>
        /// Enables all views and capabilities available.
        /// </summary>
        Full = 0,

        /// <summary>
        /// Enables only some views prompting the user to see content in the <see cref="LoginViewType.Full"/> view.
        /// </summary>
        Constrained = 1,

        /// <summary>
        /// Displays only views that are at the minimum requirement.
        /// </summary>
        Basic = 2
    }
}
