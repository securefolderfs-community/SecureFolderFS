namespace SecureFolderFS.Sdk.Enums
{
    /// <summary>
    /// Defines types of navigation.
    /// </summary>
    public enum NavigationType
    {
        /// <summary>
        /// Navigation is not correlated with the navigation timeline.
        /// </summary>
        Detached = 0,

        /// <summary>
        /// Navigating backward.
        /// </summary>
        Backward = 1,

        /// <summary>
        /// Navigating forward.
        /// </summary>
        Forward = 2
    }
}
