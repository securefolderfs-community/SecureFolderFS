using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;

namespace SecureFolderFS.AvaloniaUI.Animations.Transitions.NavigationTransitions
{
    internal abstract class NavigationTransition
    {
        public virtual Task AnimateOldContentAsync(Visual oldContent)
        {
            return new FadeTransition
            {
                Target = oldContent,
                FillMode = FillMode.None,
                Duration = TimeSpan.FromMilliseconds(100),
                Mode = FadeTransition.AnimationMode.Out
            }.RunAnimationAsync();
        }

        public abstract Task AnimateNewContentAsync(Visual newContent);
    }
}