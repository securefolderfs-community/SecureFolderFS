using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace SecureFolderFS.AvaloniaUI.Animations.Transitions.NavigationTransitions
{
    internal sealed class PopUpNavigationTransition : TransitionBase
    {
        [SetsRequiredMembers]
        public PopUpNavigationTransition()
        {
            Duration = TimeSpan.Parse("0:0:1");
            Easing = new SplineEasing(0, 0.55, 0.22, 1);
        }

        protected override Task RunAnimationAsync(IVisual target)
        {
            var transform = GetTransform<ScaleTransform>(target);
            transform.ScaleX = 0.9d;
            transform.ScaleY = 0.9d;

            var animation = GetBaseAnimation();
            animation.From = new()
            {
                new Setter
                {
                    Property = ScaleTransform.ScaleXProperty,
                    Value = 0.9d
                },
                new Setter
                {
                    Property = ScaleTransform.ScaleYProperty,
                    Value = 0.9d
                },
            };
            animation.To = new()
            {
                new Setter
                {
                    Property = ScaleTransform.ScaleXProperty,
                    Value = 1d
                },
                new Setter
                {
                    Property = ScaleTransform.ScaleYProperty,
                    Value = 1d
                },
            };

            return animation.RunAnimationAsync();
        }
    }
}
