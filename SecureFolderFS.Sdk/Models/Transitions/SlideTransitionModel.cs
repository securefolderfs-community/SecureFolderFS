using SecureFolderFS.Sdk.Enums;

namespace SecureFolderFS.Sdk.Models.Transitions
{
    public sealed class SlideTransitionModel : TransitionModel
    {
        public SlideTransitionDirection SlideTransitionDirection { get; }

        public SlideTransitionModel()
            : this(SlideTransitionDirection.ToLeft)
        {
        }

        public SlideTransitionModel(SlideTransitionDirection slideTransitionDirection)
        {
            SlideTransitionDirection = slideTransitionDirection;
        }
    }
}
