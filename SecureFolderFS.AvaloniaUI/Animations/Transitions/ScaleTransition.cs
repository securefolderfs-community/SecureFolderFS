using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using SecureFolderFS.AvaloniaUI.Extensions;

namespace SecureFolderFS.AvaloniaUI.Animations.Transitions
{
    internal class ScaleTransition : Transition
    {
        public Point From { get; set; }
        public Point To { get; set; }

        public ScaleTransition()
        {
            From = new(1, 1);
            To = new(1, 1);
        }

        protected override Task RunAnimationAsync(Visual target)
        {
            target.GetTransform<ScaleTransform>();

            var animation = GetBaseAnimation();
            animation.From = new()
            {
                new Setter(ScaleTransform.ScaleXProperty, From.X),
                new Setter(ScaleTransform.ScaleYProperty, From.Y)
            };
            animation.To = new()
            {
                new Setter(ScaleTransform.ScaleXProperty, To.X),
                new Setter(ScaleTransform.ScaleYProperty, To.Y)
            };

            return animation.RunAnimationAsync();
        }
    }
}