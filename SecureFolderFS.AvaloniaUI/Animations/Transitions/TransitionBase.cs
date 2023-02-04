using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace SecureFolderFS.AvaloniaUI.Animations.Transitions
{
    /// <summary>
    /// Base class for animations and transitions.
    /// </summary>
    internal abstract class TransitionBase : AvaloniaObject
    {
        public required TimeSpan Duration { get; init; }
        public TimeSpan Delay { get; init; } = TimeSpan.Zero;
        public Easing Easing { get; init; } = new LinearEasing();
        public FillMode FillMode { get; init; } = FillMode.Forward;

        public Task RunAnimationAsync()
        {
            if (Target is null)
                throw new ArgumentException("Target cannot be null.");

            if (Target is not IVisual visualTarget)
                throw new ArgumentException("Target must implement IVisual.");

            return RunAnimationAsync(visualTarget);
        }

        protected abstract Task RunAnimationAsync(IVisual target);

        protected AnimationExtended GetBaseAnimation()
        {
            return new AnimationExtended
            {
                Target = Target,
                Duration = Duration,
                Easing = Easing,
                FillMode = FillMode,
                Delay = Delay
            };
        }

        public static readonly StyledProperty<Animatable?> TargetProperty =
            AvaloniaProperty.Register<TransitionBase, Animatable?>(nameof(Target));

        [ResolveByName]
        public Animatable? Target
        {
            get => GetValue(TargetProperty);
            set => SetValue(TargetProperty, value);
        }
    }
}