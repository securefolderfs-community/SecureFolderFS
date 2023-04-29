using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation.Easings;

namespace SecureFolderFS.AvaloniaUI.Animations.Transitions.NavigationTransitions
{
    internal class SlideNavigationTransition : NavigationTransition
    {
        private static readonly TimeSpan Duration = TimeSpan.FromMilliseconds(300);
        private static readonly Easing Easing = new CubicEaseOut();

        /// <summary>
        /// Gets or sets the side to slide from.
        /// </summary>
        public required Side From { get; init; }

        public required double Offset { get; init; }

        /// <summary>
        /// Gets or sets whether old content should be animated before navigation.
        /// </summary>
        public bool AnimateOldContent { get; init; }

        [SetsRequiredMembers]
        public SlideNavigationTransition(Side from, double offset, bool animateOldContent = false)
        {
            From = from;
            Offset = offset;
            AnimateOldContent = animateOldContent;
        }

        public override Task AnimateOldContentAsync(Visual oldContent)
        {
            if (!AnimateOldContent)
                return Task.CompletedTask;

            return new TranslateTransition
            {
                Target = oldContent,
                Duration = Duration,
                Easing = Easing,
                From = new(0, 0),
                To = From switch
                {
                    Side.Left => new(Offset, 0),
                    Side.Right => new(-Offset, 0),
                    Side.Top => new(0, Offset),
                    Side.Bottom => new(0, -Offset),
                    _ => new Point(0, 0)
                }
            }.RunAnimationAsync();
        }

        public override Task AnimateNewContentAsync(Visual newContent)
        {
            return new TranslateTransition
            {
                Target = newContent,
                Duration = Duration,
                Easing = Easing,
                From = From switch
                {
                    Side.Left => new(-Offset, 0),
                    Side.Right => new(Offset, 0),
                    Side.Top => new(0, -Offset),
                    Side.Bottom => new(0, Offset),
                    _ => new Point(0, 0)
                },
                To = new(0, 0)
            }.RunAnimationAsync();
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