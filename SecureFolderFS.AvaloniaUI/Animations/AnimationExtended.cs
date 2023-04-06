using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Collections;
using Avalonia.Media;
using Avalonia.VisualTree;
using SecureFolderFS.AvaloniaUI.Extensions;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.AvaloniaUI.Animations
{
    internal class AnimationExtended : Animation
    {
        /// <summary>
        /// Gets or sets the animated control.
        /// </summary>
        public required Visual Target { get; set; }

        /// <summary>
        /// Gets or sets the setters of the first frame in the animation.
        /// </summary>
        /// <remarks>This property won't take effect if <see cref="AnimationExtended.Children"/> is not empty.</remarks>
        public AvaloniaList<IAnimationSetter> From { get; set; }

        /// <summary>
        /// Gets or sets the setters of the last frame in the animation.
        /// </summary>
        /// <remarks>This property won't take effect if <see cref="AnimationExtended.Children"/> is not empty.</remarks>
        public AvaloniaList<IAnimationSetter> To { get; set; }

        public AnimationExtended()
        {
            From = new();
            To = new();
        }

        public Task RunAnimationAsync()
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

            // Apply setters of the first frame immediately
            var visualTarget = (IVisual)Target;
            foreach (var keyFrame in Children.Where(keyFrame => keyFrame.Cue.CueValue == 0))
            foreach (var setter in keyFrame.Setters.Where(setter => setter.Property is not null))
            {
                if (setter.Property == TranslateTransform.XProperty || setter.Property == TranslateTransform.YProperty)
                    visualTarget.GetTransform<TranslateTransform>().SetValue(setter.Property, setter.Value);
                else if (setter.Property == ScaleTransform.ScaleXProperty || setter.Property == ScaleTransform.ScaleYProperty)
                    visualTarget.GetTransform<ScaleTransform>().SetValue(setter.Property, setter.Value);
                else
                    Target.SetValue(setter.Property!, setter.Value);
            }

            return RunAsync(Target, null);
        }
    }
}