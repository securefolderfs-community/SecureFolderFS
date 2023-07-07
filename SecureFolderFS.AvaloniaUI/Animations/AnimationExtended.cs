using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Collections;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using SecureFolderFS.AvaloniaUI.Extensions;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.AvaloniaUI.Animations
{
    // TODO Rewrite this
    internal class AnimationExtended
    {
        private readonly Animation _animation = new();
        
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
            _animation.Duration = TimeSpan.FromMicroseconds(1);
            
            if (_animation.Children.IsEmpty() && (!From.IsEmpty() || !To.IsEmpty()))
            {
                if (!From.IsEmpty())
                {
                    var from = new KeyFrame { Cue = new(0) };
                    from.Setters.AddRange(From);
                    _animation.Children.Add(from);
                }

                if (!To.IsEmpty())
                {
                    var to = new KeyFrame { Cue = new(1) };
                    to.Setters.AddRange(To);
                    _animation.Children.Add(to);
                }
            }

            // Apply setters of the first frame immediately
            foreach (var keyFrame in _animation.Children.Where(keyFrame => keyFrame.Cue.CueValue == 0))
            foreach (var setter in keyFrame.Setters.Cast<Setter>().Where(setter => setter.Property is not null))
            {
                if (setter.Property == TranslateTransform.XProperty || setter.Property == TranslateTransform.YProperty)
                    Target.GetTransform<TranslateTransform>().SetValue(setter.Property, setter.Value);
                else if (setter.Property == ScaleTransform.ScaleXProperty || setter.Property == ScaleTransform.ScaleYProperty)
                    Target.GetTransform<ScaleTransform>().SetValue(setter.Property, setter.Value);
                else
                    Target.SetValue(setter.Property!, setter.Value);
            }

            return _animation.RunAsync(Target);
        }
    }
}