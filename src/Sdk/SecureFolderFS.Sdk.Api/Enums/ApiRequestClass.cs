namespace SecureFolderFS.Sdk.Api.Enums
{
    /// <summary>
    /// Classifies a request by its expensiveness.
    /// </summary>
    public enum ApiRequestClass
    {
        /// <summary>
        /// Reads application state.
        /// </summary>
        Read,

        /// <summary>
        /// Raises a window or changes vault state.
        /// </summary>
        Trigger,

        /// <summary>
        /// Asks for consent.
        /// </summary>
        Pairing
    }
}
