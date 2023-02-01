using System.Diagnostics.CodeAnalysis;

namespace SecureFolderFS.AvaloniaUI.Animations.Transitions.NavigationTransitions
{
    internal sealed class EntranceNavigationTransition : SlideNavigationTransition
    {
        [SetsRequiredMembers]
        public EntranceNavigationTransition()
            : base(Side.Bottom, BigOffset)
        {
        }
    }
}