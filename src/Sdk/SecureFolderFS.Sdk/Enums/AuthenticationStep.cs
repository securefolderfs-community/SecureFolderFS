namespace SecureFolderFS.Sdk.Enums
{
    /// <summary>
    /// Represents the outcome of advancing the authentication sequence.
    /// </summary>
    public enum AuthenticationStep
    {
        /// <summary>
        /// Advanced to the next authentication method.
        /// </summary>
        Advanced,

        /// <summary>
        /// No more methods remain; the sequence is complete and the vault can be unlocked.
        /// </summary>
        Completed,

        /// <summary>
        /// An error occurred while advancing the sequence.
        /// </summary>
        Faulted
    }
}