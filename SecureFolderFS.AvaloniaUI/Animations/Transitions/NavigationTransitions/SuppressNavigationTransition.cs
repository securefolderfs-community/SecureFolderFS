using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Avalonia.VisualTree;

namespace SecureFolderFS.AvaloniaUI.Animations.Transitions.NavigationTransitions
{
    /// <summary>
    /// Specifies that animations are suppressed during navigation.
    /// </summary>
    internal sealed class SuppressNavigationTransition : TransitionBase
    {
        [SetsRequiredMembers]
        public SuppressNavigationTransition()
        {
            Duration = TimeSpan.Zero;
        }

        protected override Task RunAnimationAsync(IVisual target)
        {
            return Task.CompletedTask;
        }
    }
}