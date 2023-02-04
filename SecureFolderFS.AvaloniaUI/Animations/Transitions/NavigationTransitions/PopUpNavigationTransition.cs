using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Animation.Easings;

namespace SecureFolderFS.AvaloniaUI.Animations.Transitions.NavigationTransitions
{
    internal sealed class PopUpNavigationTransition : ScaleTransition
    {
        [SetsRequiredMembers]
        public PopUpNavigationTransition()
        {
            Duration = TimeSpan.Parse("0:0:1");
            Easing = new SplineEasing(0, 0.55, 0.22, 1);
            From = new(0.9, 0.9);
        }
    }
}
