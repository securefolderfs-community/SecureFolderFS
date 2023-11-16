using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;

namespace SecureFolderFS.AvaloniaUI.Animations.Transitions.NavigationTransitions
{
    internal sealed class EntranceNavigationTransition : SlideNavigationTransition
    {
        [SetsRequiredMembers]
        public EntranceNavigationTransition()
            : base(Side.Bottom, 200)
        {
        }

        public override Task AnimateOldContentAsync(Visual oldContent)
        {
            return new FadeTransition
            {
                Target = oldContent,
                Duration = TimeSpan.FromMilliseconds(100),
                FillMode = FillMode.None,
                Mode = FadeTransition.AnimationMode.Out
            }.RunAnimationAsync();
        }
    }
}