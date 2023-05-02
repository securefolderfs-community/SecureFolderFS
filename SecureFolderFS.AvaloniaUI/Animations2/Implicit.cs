using Avalonia;
using Avalonia.Collections;
using SecureFolderFS.AvaloniaUI.Animations;

namespace SecureFolderFS.AvaloniaUI.Animations2
{
    // TODO implement
    internal sealed class Implicit
    {
        public static readonly AttachedProperty<AvaloniaList<AnimationBase>> ShowAnimationsProperty =
            AvaloniaProperty.RegisterAttached<AnimationExtended, Visual, AvaloniaList<AnimationBase>>("ShowAnimations");

        public static void SetShowAnimations(Visual obj, AvaloniaList<AnimationBase> value)
        {
            obj.SetValue(ShowAnimationsProperty, value);
        }

        public static AvaloniaList<AnimationBase> GetShowAnimations(Visual obj)
        {
            var value = (AvaloniaList<AnimationBase>?)obj.GetValue(ShowAnimationsProperty);
            if (value is null)
            {
                value = new();
                obj.SetValue(ShowAnimationsProperty, value);
            }

            return value;
        }

        public static readonly AttachedProperty<AvaloniaList<AnimationBase>> HideAnimationsProperty =
            AvaloniaProperty.RegisterAttached<AnimationExtended, Visual, AvaloniaList<AnimationBase>>("HideAnimations");

        public static void SetHideAnimations(Visual obj, AvaloniaList<AnimationBase> value)
        {
            obj.SetValue(HideAnimationsProperty, value);
        }

        public static AvaloniaList<AnimationBase> GetHideAnimations(Visual obj)
        {
            var value = (AvaloniaList<AnimationBase>?)obj.GetValue(HideAnimationsProperty);
            if (value is null)
            {
                value = new();
                obj.SetValue(HideAnimationsProperty, value);
            }

            return value;
        }
    }
}