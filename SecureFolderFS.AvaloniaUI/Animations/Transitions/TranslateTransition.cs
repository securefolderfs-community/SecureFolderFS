using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using SecureFolderFS.AvaloniaUI.Extensions;

namespace SecureFolderFS.AvaloniaUI.Animations.Transitions
{
    internal sealed class TranslateTransition : Transition
    {
        public Point From { get; set; }
        public Point To { get; set; }

        public TranslateTransition()
        {
            From = new(0, 0);
            To = new(0, 0);
        }

        protected override Task RunAnimationAsync(Visual target)
        {
            target.GetTransform<TranslateTransform>();

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