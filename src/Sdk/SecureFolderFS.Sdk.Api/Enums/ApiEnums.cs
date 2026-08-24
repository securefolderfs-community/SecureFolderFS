namespace SecureFolderFS.Sdk.Api.Enums
{
    /// <summary>
    /// Identifies the local IPC mechanism carrying a connection.
    /// </summary>
    public enum ApiTransportKind
    {
        /// <summary>
        /// A Windows named pipe, restricted by an explicit DACL.
        /// </summary>
        NamedPipe,

        /// <summary>
        /// A Unix domain socket, restricted by filesystem permissions.
        /// </summary>
        UnixSocket
    }

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

    /// <summary>
    /// Classifies a request by its rate-limit budget.
    /// </summary>
    public enum ApiRequestType
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

    /// <summary>
    /// Describes the kind of change that occurred to a vault.
    /// </summary>
    public enum ApiVaultChangeKind
    {
        /// <summary>
        /// A vault became visible to integrations.
        /// </summary>
        Added,

        /// <summary>
        /// A vault stopped being visible to integrations, whether removed or hidden.
        /// </summary>
        Removed,

        /// <summary>
        /// A vault's display name changed.
        /// </summary>
        Renamed,

        /// <summary>
        /// A vault was mounted.
        /// </summary>
        Unlocked,

        /// <summary>
        /// A vault was unmounted.
        /// </summary>
        Locked
    }
}
