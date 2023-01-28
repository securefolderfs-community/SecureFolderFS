using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;

namespace SecureFolderFS.AvaloniaUI.Animations
{
    /// <summary>
    /// Common animations used in multiple controls.
    /// </summary>
    internal static class CommonAnimations
    {
        public static IEnumerable<Animation> CreateEmergeInfoBarFromBannerAnimation(Animatable infoBar, Animatable contentBelow)
        {
            return new List<Animation>
            {
                new()
                {
                    Duration = TimeSpan.Parse("0:0:0.4"),
                    Target = infoBar,
                    FillMode = FillMode.Forward,
                    Easing = new QuinticEaseOut(),
                    From = { new Setter(TranslateTransform.YProperty, -((IVisual)infoBar).Bounds.Height) },
                    To = { new Setter(TranslateTransform.YProperty, 0d) }
                },
                new()
                {
                    Duration = TimeSpan.Parse("0:0:0.6"),
                    Target = infoBar,
                    FillMode = FillMode.Forward,
                    Easing = new QuinticEaseOut(),
                    From = { new Setter(Visual.OpacityProperty, 0d) },
                    To = { new Setter(Visual.OpacityProperty, 1d) }
                },
                new()
                {
                    Duration = TimeSpan.Parse("0:0:0.4"),
                    Target = contentBelow,
                    FillMode = FillMode.Forward,
                    Easing = new QuinticEaseOut(),
                    From = { new Setter(TranslateTransform.YProperty, -((IVisual)infoBar).Bounds.Height) },
                    To = { new Setter(TranslateTransform.YProperty, 0d) }
                }
            };
        }

        public static IEnumerable<Animation> CreateMergeInfoBarIntoBannerAnimation(Animatable infoBar, Animatable contentBelow, bool quick = false)
        {
            var infoBarTranslateDuration = quick ? TimeSpan.Parse("0:0:0.2") : TimeSpan.Parse("0:0:0.4");
            return new List<Animation>
            {
                new()
                {
                    Duration = infoBarTranslateDuration,
                    Target = infoBar,
                    FillMode = FillMode.Forward,
                    Easing = new QuinticEaseOut(),
                    From = { new Setter(TranslateTransform.YProperty, 0d) },
                    To = { new Setter(TranslateTransform.YProperty, -((IVisual)infoBar).Bounds.Height) }
                },
                new()
                {
                    Duration = infoBarTranslateDuration,
                    Target = infoBar,
                    FillMode = FillMode.Forward,
                    Easing = new QuinticEaseOut(),
                    From = { new Setter(Visual.OpacityProperty, 1d) },
                    To = { new Setter(Visual.OpacityProperty, 0d) }
                },
                new()
                {
                    Duration = quick ? TimeSpan.Parse("0:0:0.2") : TimeSpan.Parse("0:0:0.6"),
                    Target = contentBelow,
                    FillMode = FillMode.Forward,
                    Easing = new QuinticEaseOut(),
                    From = { new Setter(TranslateTransform.YProperty, 0d) },
                    To = { new Setter(TranslateTransform.YProperty, -((IVisual)infoBar).Bounds.Height) }
                }
            };
        }

        public static Animation CreateSpinningIconAnimation(Animatable icon)
        {
            return new Animation
            {
                Duration = TimeSpan.Parse("0:0:1"),
                Target = icon,
                Easing = new BackEaseInOut(),
                From = { new Setter(RotateTransform.AngleProperty, 0d) },
                To = { new Setter(RotateTransform.AngleProperty, 360d) }
            };
        }
    }
}