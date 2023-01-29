using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace SecureFolderFS.AvaloniaUI.Animations.Transitions
{
    internal sealed class ScaleTransition : TransitionBase
    {
        public Point From { get; set; } = new(1, 1);
        public Point To { get; set; } = new(1, 1);

        protected override Task RunAnimationAsync(IVisual target)
        {
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