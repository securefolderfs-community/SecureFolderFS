using System.Threading.Tasks;
using Avalonia;
using Avalonia.Styling;

namespace SecureFolderFS.AvaloniaUI.Animations2
{
    internal sealed class DoubleAnimation : AnimationBase
    {
        public double From { get; init; }
        public double To { get; init; }

        protected override Task BeginInternalAsync(Visual target, AvaloniaProperty targetProperty)
        {
            var animation = GetAnimation(target);
            animation.From = new() { new Setter(targetProperty, From) };
            animation.To = new() { new Setter(targetProperty, To) };

            return animation.RunAnimationAsync();
        }
    }
}