using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Animation;
using Avalonia.Collections;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.AvaloniaUI.Animations
{
    internal sealed class Animation : Avalonia.Animation.Animation
    {
        /// <summary>
        /// Gets or sets the animated control.
        /// </summary>
        public required Animatable Target { get; set; }

        /// <summary>
        /// Gets or sets the setters of the first frame in the animation.
        /// </summary>
        /// <remarks>This property won't take effect if <see cref="Animation.Children"/> is not empty.</remarks>
        public AvaloniaList<IAnimationSetter> From { get; set; }

        /// <summary>
        /// Gets or sets the setters of the first frame in the animation.
        /// </summary>
        /// <remarks>This property won't take effect if <see cref="Animation.Children"/> is not empty.</remarks>
        public AvaloniaList<IAnimationSetter> To { get; set; }

        public Animation()
        {
            From = new();
            To = new();
        }

        public Task RunAsync()
        {
            if (Children.IsEmpty() && (!From.IsEmpty() || !To.IsEmpty()))
            {
                if (!From.IsEmpty())
                {
                    var from = new KeyFrame { Cue = new(0) };
                    from.Setters.AddRange(From);
                    Children.Add(from);
                }

                if (!To.IsEmpty())
                {
                    var to = new KeyFrame { Cue = new(1) };
                    to.Setters.AddRange(To);
                    Children.Add(to);
                }
            }

            return RunAsync(Target, null);
        }

        /// <summary>
        /// Runs all animations in the provided list in parallel.
        /// </summary>
        public static Task RunAsync(IEnumerable<Animation> animations)
        {
            return Task.WhenAll(animations.Select(x => x.RunAsync()));
        }
    }
}