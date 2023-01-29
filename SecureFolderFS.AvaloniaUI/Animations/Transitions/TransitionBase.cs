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

        protected TTransform GetTransform<TTransform>(IVisual control)
            where TTransform : Transform
        {
            if (control.RenderTransform is TTransform targetTransform)
                return targetTransform;

            TTransform? transform = null;
            if (control.RenderTransform is TransformGroup transformGroup)
                transform = (TTransform?)transformGroup.Children?.FirstOrDefault(x => x is TTransform);

            if (transform is null)
                throw new ArgumentException($"Target must have a {typeof(TTransform)}.");

            return transform;
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