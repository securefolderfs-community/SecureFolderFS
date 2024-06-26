namespace SecureFolderFS.Sdk.Enums
{
    /// <summary>
    /// Determines which stage a given authentication method can be applied to.
    /// </summary>
    public enum AuthenticationType
    {
        /// <summary>
        /// The authentication method may only be used as the first stage.
        /// </summary>
        FirstStageOnly = 1,

        /// <summary>
        /// The authentication method may only be used as another stage.
        /// </summary>
        LatterStageOnly = 2,

        /// <summary>
        /// The authentication method can be used in any stage.
        /// </summary>
        Any = FirstStageOnly | LatterStageOnly
    }
}
