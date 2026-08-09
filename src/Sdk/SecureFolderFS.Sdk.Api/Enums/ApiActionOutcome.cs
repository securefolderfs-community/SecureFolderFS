namespace SecureFolderFS.Sdk.Api.Enums
{
    /// <summary>
    /// Describes the outcome of an action requested through the API.
    /// </summary>
    public enum ApiActionOutcome
    {
        /// <summary>
        /// The action was performed, or the requested user interface was shown.
        /// </summary>
        Ok,

        /// <summary>
        /// A prompt for this vault was already open, so no additional one was raised.
        /// </summary>
        AlreadyPending,

        /// <summary>
        /// The vault was already in the requested state.
        /// </summary>
        NoChange,

        /// <summary>
        /// No vault with the supplied identifier is visible to integrations.
        /// </summary>
        NotFound,

        /// <summary>
        /// The action does not apply while the vault is in its current state.
        /// </summary>
        InvalidState,

        /// <summary>
        /// The application is not ready to service the request.
        /// </summary>
        Unavailable
    }
}
