using System;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using SecureFolderFS.Backend.Extensions;
using SecureFolderFS.WinUI.Enums;

namespace SecureFolderFS.WinUI.Helpers
{
    internal static class AnimationHelpers
    {
        public static void EnableImplicitAnimation(UIElement element, VisualPropertyType typeToAnimate,
            double duration = 800, double delay = 0, CompositionEasingFunction easing = null)
        {
            var visual = ElementCompositionPreview.GetElementVisual(element);
            var compositor = visual.Compositor;

            var animationCollection = compositor.CreateImplicitAnimationCollection();

            foreach (var type in EnumExtensions.GetValues<VisualPropertyType>())
            {
                if (!typeToAnimate.HasFlag(type)) continue;

                var animation = CreateAnimationByType(compositor, type, duration, delay, easing);

                if (animation is not null)
                {
                    animationCollection[type.ToString()] = animation;
                }
            }

            visual.ImplicitAnimations = animationCollection;
        }

        private static KeyFrameAnimation CreateAnimationByType(Compositor compositor, VisualPropertyType type,
            double duration = 800, double delay = 0, CompositionEasingFunction easing = null)
        {
            KeyFrameAnimation animation;

            switch (type)
            {
                case VisualPropertyType.Offset:
                case VisualPropertyType.Scale:
                    animation = compositor.CreateVector3KeyFrameAnimation();
                    break;
                case VisualPropertyType.Size:
                    animation = compositor.CreateVector2KeyFrameAnimation();
                    break;
                case VisualPropertyType.Opacity:
                case VisualPropertyType.RotationAngleInDegrees:
                    animation = compositor.CreateScalarKeyFrameAnimation();
                    break;
                default:
                    return null;
            }

            animation.InsertExpressionKeyFrame(1.0f, "this.FinalValue", easing);
            animation.Duration = TimeSpan.FromMilliseconds(duration);
            animation.DelayTime = TimeSpan.FromMilliseconds(delay);
            animation.Target = type.ToString();

            return animation;
        }
    }
}
