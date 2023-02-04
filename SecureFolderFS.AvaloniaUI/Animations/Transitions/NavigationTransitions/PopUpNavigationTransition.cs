using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation.Easings;

namespace SecureFolderFS.AvaloniaUI.Animations.Transitions.NavigationTransitions
{
    internal sealed class PopUpNavigationTransition : NavigationTransition
    {
        public override Task AnimateNewContentAsync(Visual newContent)
        {
            return new ScaleTransition
            {
                Target = newContent,
                Duration = TimeSpan.FromSeconds(1),
                Easing = new SplineEasing(0, 0.55, 0.22, 1),
                From = new(0.9, 0.9)
            }.RunAnimationAsync();
        }
    }
}
