#region

using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;

#endregion

namespace SecureFolderFS.AvaloniaUI.Animations.Transitions
{
    /// <remarks>This transition is not affected by duration.</remarks>
    internal sealed class InfoBarTransition : TransitionBase
    {
        public required Mode AnimationMode { get; set; }

        protected override Task RunAnimationAsync(IVisual target)
        {
            if (ContentBelow is not IVisual contentBelow)
                throw new ArgumentException("ContentBelow must implement IVisual.");

            TransitionBase infoBarOpacityTransition;
            TransitionBase infoBarTranslateTransition;
            TransitionBase contentBelowTranslateTransition;

            if (AnimationMode == Mode.Show)
            {
                GetTransform<TranslateTransform>(contentBelow).Y = -target.Bounds.Height;
                infoBarOpacityTransition = new FadeTransition
                {
                    Target = Target,
                    Duration = TimeSpan.FromMilliseconds(600),
                    Easing = new QuinticEaseOut(),
                    Mode = FadeTransition.AnimationMode.In
                };
                infoBarTranslateTransition = new TranslateTransition
                {
                    Target = Target,
                    Duration = TimeSpan.FromMilliseconds(400),
                    Easing = new QuinticEaseOut(),
                    From = new(0, -target.Bounds.Height)
                };
                contentBelowTranslateTransition = new TranslateTransition
                {
                    Target = ContentBelow,
                    Duration = TimeSpan.FromMilliseconds(400),
                    Easing = new QuinticEaseOut(),
                    From = new(0, -target.Bounds.Height)
                };
            }
            else
            {
                infoBarOpacityTransition = new FadeTransition
                {
                    Target = Target,
                    Duration = TimeSpan.FromMilliseconds(AnimationMode == Mode.QuickHide ? 200 : 600),
                    Easing = new QuinticEaseOut(),
                    Mode = FadeTransition.AnimationMode.Out
                };
                infoBarTranslateTransition = new TranslateTransition
                {
                    Target = Target,
                    Duration = TimeSpan.FromMilliseconds(AnimationMode == Mode.QuickHide ? 200 : 400),
                    Easing = new QuinticEaseOut(),
                    To = new(0, -target.Bounds.Height)
                };
                contentBelowTranslateTransition = new TranslateTransition()
                {
                    Target = ContentBelow,
                    Duration = TimeSpan.FromMilliseconds(AnimationMode == Mode.QuickHide ? 200 : 400),
                    Easing = new QuinticEaseOut(),
                    To = new(0, -target.Bounds.Height)
                };
            }

            return Task.WhenAll
            (
                infoBarOpacityTransition.RunAnimationAsync(),
                infoBarTranslateTransition.RunAnimationAsync(),
                contentBelowTranslateTransition.RunAnimationAsync()
            );
        }

        public static readonly StyledProperty<Animatable> ContentBelowProperty =
            AvaloniaProperty.Register<InfoBarTransition, Animatable>(nameof(ContentBelow));

        [ResolveByName]
        public Animatable ContentBelow
        {
            get => GetValue(ContentBelowProperty);
            set => SetValue(ContentBelowProperty, value);
        }

        public enum Mode
        {
            Show,
            Hide,
            QuickHide
        }
    }
}