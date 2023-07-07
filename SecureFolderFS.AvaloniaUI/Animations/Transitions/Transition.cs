using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;

namespace SecureFolderFS.AvaloniaUI.Animations.Transitions
{
    /// <summary>
    /// Base class for transitions.
    /// </summary>
    internal abstract class Transition : AvaloniaObject
    {
        public required TimeSpan Duration { get; init; }
        public TimeSpan Delay { get; init; }
        public Easing Easing { get; init; }
        public FillMode FillMode { get; init; }

        protected Transition()
        {
            Delay = TimeSpan.Zero;
            Easing = new LinearEasing();
            FillMode = FillMode.Forward;
        }

        /// <exception cref="ArgumentException">Target is null.</exception>
        public Task RunAnimationAsync()
        {
            if (Target is null)
                throw new ArgumentException("Target cannot be null.");

            return RunAnimationAsync(Target);
        }

        protected abstract Task RunAnimationAsync(Visual target);

        /// <exception cref="ArgumentException">Target is null.</exception>
        protected AnimationExtended GetBaseAnimation()
        {
            if (Target is null)
                throw new ArgumentException("Target cannot be null.");

            return new AnimationExtended
            {
                Target = Target,
                // Duration = Duration,
                // Easing = Easing,
                // FillMode = FillMode,
                // Delay = Delay
            };
        }

        public static readonly StyledProperty<Visual?> TargetProperty =
            AvaloniaProperty.Register<Transition, Visual?>(nameof(Target));

        [ResolveByName]
        public Visual? Target
        {
            get => GetValue(TargetProperty);
            set => SetValue(TargetProperty, value);
        }
    }
}