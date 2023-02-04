using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using SecureFolderFS.AvaloniaUI.Extensions;

namespace SecureFolderFS.AvaloniaUI.Animations.Transitions
{
    internal sealed class RotateTransition : TransitionBase
    {
        public double From { get; set; }
        public double To { get; set; }

        protected override Task RunAnimationAsync(IVisual target)
        {
            target.GetTransform<RotateTransform>();
            var animation = GetBaseAnimation();
            animation.From = new() { new Setter(RotateTransform.AngleProperty, From) };
            animation.To = new() { new Setter(RotateTransform.AngleProperty, To) };

            return animation.RunAnimationAsync();
        }
    }
}