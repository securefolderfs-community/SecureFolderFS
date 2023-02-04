using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using SecureFolderFS.AvaloniaUI.Animations.Transitions;

namespace SecureFolderFS.AvaloniaUI.Animations
{
    /// <summary>
    /// A collection of animations, attachable to any control.
    /// </summary>
    internal sealed class Storyboard : Control
    {
        public Storyboard()
        {
            Animations = new();
        }

        public Task RunAnimationsAsync()
        {
            return Task.WhenAll(Animations.Select(x => x.RunAnimationAsync()));
        }

        // This method exists to make it possible to run the transition using Interactions
        public void RunAnimations(object? sender, RoutedEventArgs e)
        {
            RunAnimationsAsync();
        }

        public static readonly StyledProperty<AvaloniaList<Transition>> AnimationsProperty =
            AvaloniaProperty.Register<Storyboard, AvaloniaList<Transition>>(nameof(Animations));

        [Content]
        public AvaloniaList<Transition> Animations
        {
            get => GetValue(AnimationsProperty);
            set => SetValue(AnimationsProperty, value);
        }

        public static readonly AttachedProperty<AvaloniaList<Storyboard>> StoryboardsProperty =
            AvaloniaProperty.RegisterAttached<AnimationExtended, IAvaloniaObject, AvaloniaList<Storyboard>>("Storyboards");

        public static void SetStoryboards(IAvaloniaObject obj, AvaloniaList<Storyboard> value)
        {
            obj.SetValue(StoryboardsProperty, value);
        }

        public static AvaloniaList<Storyboard> GetStoryboards(IAvaloniaObject obj)
        {
            var value = (AvaloniaList<Storyboard>?)obj.GetValue(StoryboardsProperty);
            if (value is null)
            {
                value = new();
                obj.SetValue(StoryboardsProperty, value);
            }

            return value;
        }
    }
}