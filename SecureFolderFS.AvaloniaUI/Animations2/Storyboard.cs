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

        public static readonly AttachedProperty<Visual> TargetNameProperty =
            AvaloniaProperty.RegisterAttached<Storyboard, AnimationBase, Visual>("TargetName");

        [ResolveByName]
        public static void SetTargetName(AnimationBase obj, Visual value)
        {
            obj.SetValue(TargetNameProperty, value);
        }

        public static Visual GetTargetName(AnimationBase obj)
        {
            return obj.GetValue(TargetNameProperty);
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