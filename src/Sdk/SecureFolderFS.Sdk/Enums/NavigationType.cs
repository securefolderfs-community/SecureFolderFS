namespace SecureFolderFS.Sdk.Enums
{
    /// <summary>
    /// Defines types of navigation.
    /// </summary>
    public enum NavigationType
    {
        /// <summary>
        /// Navigation is performed to an arbitrary target which may or may not preserve the navigation timeline.
        /// </summary>
        Chained = 0,

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
