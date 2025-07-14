using System;

namespace SecureFolderFS.Sdk.Enums
{
    /// <summary>
    /// Determines which stage a given authentication method can be applied to.
    /// </summary>
    [Flags]
    public enum AuthenticationStage : uint
    {
        /// <summary>
        /// The authentication method may only be used as the first stage.
        /// </summary>
        FirstStageOnly = 1u,

        /// <summary>
        /// The authentication method may only be used as another stage.
        /// </summary>
        ProceedingStageOnly = 2u,

        /// <summary>
        /// The authentication method can be used in any stage.
        /// </summary>
        Any = FirstStageOnly | ProceedingStageOnly
    }
}
