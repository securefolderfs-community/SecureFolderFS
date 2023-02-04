using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Collections;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactions.Events;
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

        /// <summary>
        /// Whether to slide out the content from the opposite side, as specified in <see cref="From"/>, when navigating from the page.
        /// </summary>
        public bool SlideOutWhenNavigatingFrom { get; set; }

        [SetsRequiredMembers]
        public SlideNavigationTransition(Side from, double offset, bool slideOutWhenNavigatingFrom = false)
        {
            Duration = TimeSpan.Parse("0:0:0:0.3");
            Easing = new CubicEaseOut();
            From = from;
            Offset = offset;
        }

        protected override async Task RunAnimationAsync(IVisual target)
        {
            target.GetTransform<TranslateTransform>();
            AvaloniaList<IAnimationSetter> fromSetters = null!;

            if (SlideOutWhenNavigatingFrom)
            {
                await SlideAsync(target, From switch
                {
                    Side.Left => Side.Right,
                    Side.Right => Side.Left,
                    Side.Top => Side.Bottom,
                    Side.Bottom => Side.Top,
                });
            }

            await SlideAsync(target, From);
        }

        private Task SlideAsync(IVisual target, Side side)
        {
            target.GetTransform<TranslateTransform>();

            var animation = GetBaseAnimation();
            animation.From = From switch
            {
                Side.Left => new(new Setter(TranslateTransform.XProperty, -Offset)),
                Side.Right => new(new Setter(TranslateTransform.XProperty, Offset)),
                Side.Top => new(new Setter(TranslateTransform.YProperty, -Offset)),
                Side.Bottom => new(new Setter(TranslateTransform.YProperty, Offset))
            };
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