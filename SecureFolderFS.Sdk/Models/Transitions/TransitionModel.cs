using System;

namespace SecureFolderFS.Sdk.Models.Transitions
{
    [Obsolete("This class has been deprecated. The SecureFolderFS.Sdk should know nothing about the animation.")]
    public abstract class TransitionModel
    {
        public bool IsCustom { get; protected set; } = false;
    }
}
