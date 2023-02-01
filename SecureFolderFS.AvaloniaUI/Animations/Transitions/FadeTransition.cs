using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace SecureFolderFS.AvaloniaUI.Animations.Transitions
{
    internal sealed class FadeTransition : TransitionBase
    {
        public required AnimationMode Mode { get; set; }

        /// <summary>
        /// Gets or sets whether to update visibility of the target before and after running the animation.
        /// </summary>
        public bool UpdateVisibility { get; set; }

        /// <exception cref="ArgumentException">Target doesn't implement IVisual</exception>
        protected override async Task RunAnimationAsync(IVisual target)
        {
            var animation = GetBaseAnimation();
            animation.From = new(new Setter(Visual.OpacityProperty, Mode == AnimationMode.In ? 0d : 1d));
            animation.To = new(new Setter(Visual.OpacityProperty, Mode == AnimationMode.In ? 1d : 0d));

            if (UpdateVisibility && Mode == AnimationMode.In)
                target.IsVisible = true;

            await animation.RunAnimationAsync();

            if (UpdateVisibility && Mode == AnimationMode.Out)
                target.IsVisible = false;
        }

        public enum AnimationMode
        {
            In,
            Out
        }
    }
}