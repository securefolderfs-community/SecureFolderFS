using SecureFolderFS.Backend.Enums;

namespace SecureFolderFS.Backend.Models.Transitions
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
            this.SlideTransitionDirection = slideTransitionDirection;
        }
    }
}
