using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace SecureFolderFS.AvaloniaUI.Animations.Transitions
{
    internal sealed class TranslateTransition : TransitionBase
    {
        public Point From { get; set; } = new(0, 0);
        public Point To { get; set; } = new(0, 0);

        protected override Task RunAnimationAsync(IVisual target)
        {
            var animation = GetBaseAnimation();
            animation.From = new()
            {
                new Setter(TranslateTransform.XProperty, From.X),
                new Setter(TranslateTransform.YProperty, From.Y)
            };
            animation.To = new()
            {
                new Setter(TranslateTransform.XProperty, To.X),
                new Setter(TranslateTransform.YProperty, To.Y)
            };

            return animation.RunAnimationAsync();
        }
    }
}