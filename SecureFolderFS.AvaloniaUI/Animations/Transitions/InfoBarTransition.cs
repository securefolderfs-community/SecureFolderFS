using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using SecureFolderFS.AvaloniaUI.Extensions;

namespace SecureFolderFS.AvaloniaUI.Animations.Transitions
{
    /// <remarks>This transition is not affected by duration.</remarks>
    internal sealed class InfoBarTransition : Transition
    {
        public required AnimationMode Mode { get; set; }

        protected override Task RunAnimationAsync(Visual target)
        {
            Transition infoBarOpacityTransition;
            Transition infoBarTranslateTransition;
            Transition contentBelowTranslateTransition;

            if (Mode == AnimationMode.Show)
            {
                infoBarOpacityTransition = new FadeTransition
                {
                    Target = Target,
                    Duration = TimeSpan.FromMilliseconds(600),
                    Easing = new QuinticEaseOut(),
                    Mode = FadeTransition.AnimationMode.In,
                    UpdateVisibility = true
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
                    Duration = TimeSpan.FromMilliseconds(Mode == AnimationMode.QuickHide ? 200 : 600),
                    Easing = new QuinticEaseOut(),
                    Mode = FadeTransition.AnimationMode.Out,
                    UpdateVisibility = true
                };
                infoBarTranslateTransition = new TranslateTransition
                {
                    Target = Target,
                    Duration = TimeSpan.FromMilliseconds(Mode == AnimationMode.QuickHide ? 200 : 400),
                    Easing = new QuinticEaseOut(),
                    To = new(0, -target.Bounds.Height)
                };
                contentBelowTranslateTransition = new TranslateTransition
                {
                    Target = ContentBelow,
                    Duration = TimeSpan.FromMilliseconds(Mode == AnimationMode.QuickHide ? 200 : 400),
                    Easing = new QuinticEaseOut(),
                    To = new(0, -target.Bounds.Height)
                };
            }

            return Task.WhenAll
            (
                Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await infoBarOpacityTransition.RunAnimationAsync();
                    ContentBelow.GetTransform<TranslateTransform>().Y = 0;
                }),
                infoBarTranslateTransition.RunAnimationAsync(),
                contentBelowTranslateTransition.RunAnimationAsync()
            );
        }

        public static readonly StyledProperty<Visual> ContentBelowProperty =
            AvaloniaProperty.Register<InfoBarTransition, Visual>(nameof(ContentBelow));

        [ResolveByName]
        public Visual ContentBelow
        {
            get => GetValue(ContentBelowProperty);
            set => SetValue(ContentBelowProperty, value);
        }

        public enum AnimationMode
        {
            Show,
            Hide,
            QuickHide
        }
    }
}