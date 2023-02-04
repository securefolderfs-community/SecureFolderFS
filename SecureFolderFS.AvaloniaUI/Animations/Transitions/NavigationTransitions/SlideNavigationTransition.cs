using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Collections;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using SecureFolderFS.AvaloniaUI.Extensions;

namespace SecureFolderFS.AvaloniaUI.Animations.Transitions.NavigationTransitions
{
    internal class SlideNavigationTransition : TransitionBase
    {
        public const double BigOffset = 200d;
        public const double SmallOffset = 100d;

        /// <summary>
        /// Gets or sets the side to slide from.
        /// </summary>
        public required Side From { get; init; }

        public required double Offset { get; init; }

        [SetsRequiredMembers]
        public SlideNavigationTransition(Side from, double offset)
        {
            Duration = TimeSpan.Parse("0:0:0:0.3");
            Easing = new CubicEaseOut();
            From = from;
            Offset = offset;
        }

        protected override Task RunAnimationAsync(IVisual target)
        {
            target.GetTransform<TranslateTransform>();
            AvaloniaList<IAnimationSetter> fromSetters = null!;

            switch (From)
            {
                case Side.Left:
                    fromSetters = new(new Setter(TranslateTransform.XProperty, -Offset));
                    break;

                case Side.Right:
                    fromSetters = new(new Setter(TranslateTransform.XProperty, Offset));
                    break;

                case Side.Top:
                    fromSetters = new(new Setter(TranslateTransform.YProperty, -Offset));
                    break;

                case Side.Bottom:
                    fromSetters = new(new Setter(TranslateTransform.YProperty, Offset));
                    break;
            }

            var animation = GetBaseAnimation();
            animation.From = fromSetters;
            animation.To = From switch
            {
                Side.Left or Side.Right => new(new Setter(TranslateTransform.XProperty, 0d)),
                Side.Top or Side.Bottom => new(new Setter(TranslateTransform.YProperty, 0d))
            };

            return animation.RunAnimationAsync();
        }

        public enum Side
        {
            Left,
            Right,
            Top,
            Bottom
        }
    }
}