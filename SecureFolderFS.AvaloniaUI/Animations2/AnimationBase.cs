using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using SecureFolderFS.AvaloniaUI.Animations;

namespace SecureFolderFS.AvaloniaUI.Animations2
{
    internal abstract class AnimationBase : AvaloniaObject
    {
        public TimeSpan BeginTime { get; init; } = TimeSpan.Zero;
        public required TimeSpan Duration { get; init; }
        public Easing EasingFunction { get; init; } = new LinearEasing();
        public FillMode FillMode { get; init; } = FillMode.Forward;

        public Task BeginAsync()
        {
            return BeginInternalAsync(GetValue(Storyboard.TargetNameProperty), GetValue(Storyboard.TargetPropertyProperty));
        }

        protected abstract Task BeginInternalAsync(Visual target, AvaloniaProperty property);

        /// <exception cref="ArgumentException">Target is null.</exception>
        /// <returns>An instance of <see cref="AnimationExtended"/>.</returns>
        protected AnimationExtended GetAnimation(Visual target)
        {
            if (target is null)
                throw new ArgumentException("Target cannot be null.");

            return new AnimationExtended
            {
                Target = target,
                Duration = Duration,
                Easing = EasingFunction,
                FillMode = FillMode,
                Delay = BeginTime
            };
        }
    }
}