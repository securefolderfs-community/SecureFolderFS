using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Metadata;

namespace SecureFolderFS.AvaloniaUI.Animations2
{
    internal sealed class Storyboard : Control
    {
        public Storyboard()
        {
            Animations = new();
        }

        public Task BeginAsync()
        {
            return Task.WhenAll(Animations.Select(x => x.BeginAsync()));
        }

        public void Stop()
        {
            // TODO: Implement
        }

        public static readonly StyledProperty<AvaloniaList<AnimationBase>> AnimationsProperty =
            AvaloniaProperty.Register<Storyboard, AvaloniaList<AnimationBase>>(nameof(Animations));

        [Content]
        public AvaloniaList<AnimationBase> Animations
        {
            get => GetValue(AnimationsProperty);
            set => SetValue(AnimationsProperty, value);
        }

        public static readonly AttachedProperty<AvaloniaList<Storyboard>> StoryboardsProperty =
            AvaloniaProperty.RegisterAttached<Storyboard, IAvaloniaObject, AvaloniaList<Storyboard>>("Storyboards");

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

        public static readonly AttachedProperty<Visual> TargetProperty =
            AvaloniaProperty.RegisterAttached<Storyboard, AnimationBase, Visual>("Target");

        public static void SetTarget(AnimationBase obj, Visual value)
        {
            obj.SetValue(TargetProperty, value);
        }

        public static Visual GetTarget(AnimationBase obj)
        {
            return obj.GetValue(TargetProperty);
        }

        public static readonly AttachedProperty<AvaloniaProperty> TargetPropertyProperty =
            AvaloniaProperty.RegisterAttached<Storyboard, AnimationBase, AvaloniaProperty>("TargetProperty");

        public static void SetTargetProperty(AnimationBase obj, AvaloniaProperty value)
        {
            obj.SetValue(TargetPropertyProperty, value);
        }

        public static AvaloniaProperty GetTargetProperty(IAvaloniaObject obj)
        {
            return (AvaloniaProperty)obj.GetValue(TargetPropertyProperty)!;
        }
    }
}