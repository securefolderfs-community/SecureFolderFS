using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SecureFolderFS.Uno.UserControls.Introduction
{
    public sealed partial class AuthenticationSlide : UserControl
    {
        public AuthenticationSlide()
        {
            InitializeComponent();
            SetupInitialState();
        }

        private void SetupInitialState()
        {
            ResetAnimationState();
        }

        private void ResetAnimationState()
        {
            // Reset path to fully hidden
            AuthenticationPath.Opacity = 0d;

            // Reset all icons to the initially hidden state
            ResetIcon(PasswordBorder);
            ResetIcon(DeviceLinkBorder);
            ResetIcon(YubiKeyBorder);
            ResetIcon(KeyFileBorder);
        }

        private static void ResetIcon(Border icon)
        {
            // The static scale stays at 1.0 (opacity hides the icon) so WinUI rasterizes
            // the content at full resolution; a static 0.4 would make the composition
            // surface low-res and the pop animation would scale it up blurry. The
            // storyboard's From=0.4 provides the visual starting point instead.
            icon.Opacity = 0d;
            if (icon.RenderTransform is ScaleTransform scale)
            {
                scale.ScaleX = 1d;
                scale.ScaleY = 1d;
            }
        }

        private void IconsGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RebuildPathGeometry();
        }

        /// <summary>
        /// Connects the centers of the four icon circles with smooth S-curves. Built from the
        /// actual layout positions so the connection points stay aligned regardless of sizing.
        /// </summary>
        private void RebuildPathGeometry()
        {
            if (DiagramGrid.ActualWidth <= 0d || DiagramGrid.ActualHeight <= 0d)
                return;

            Point[] centers =
            [
                CenterOf(PasswordBorder),
                CenterOf(DeviceLinkBorder),
                CenterOf(YubiKeyBorder),
                CenterOf(KeyFileBorder)
            ];

            var figure = new PathFigure { StartPoint = centers[0], IsClosed = false, IsFilled = false };
            for (var i = 1; i < centers.Length; i++)
            {
                // Vertical tangents at both ends make the dashed line leave and enter each circle straight down
                var previous = centers[i - 1];
                var current = centers[i];
                var middleY = (previous.Y + current.Y) / 2d;

                figure.Segments.Add(new BezierSegment
                {
                    Point1 = new Point(previous.X, middleY),
                    Point2 = new Point(current.X, middleY),
                    Point3 = current
                });
            }

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            AuthenticationPath.Data = geometry;

            Point CenterOf(FrameworkElement element)
            {
                return element
                    .TransformToVisual(DiagramGrid)
                    .TransformPoint(new Point(element.ActualWidth / 2d, element.ActualHeight / 2d));
            }
        }

        /// <summary>
        /// Animates the four icons (tiles) to pop into view from top to bottom (staggered).
        /// Once all icons have finished animating, the S-curve path fades in to 25% opacity.
        /// Fully resets all values before starting.
        /// </summary>
        public async Task AnimateAsync()
        {
            // Always reset everything first so the animation can be replayed cleanly
            ResetAnimationState();

            var storyboard = new Storyboard();

            // Icon pop animations

            // Timings chosen so the icons appear gradually down the S-curve
            AddIconPopAnimation(storyboard, PasswordBorder, beginTimeMs: 50);
            AddIconPopAnimation(storyboard, DeviceLinkBorder, beginTimeMs: 600);
            AddIconPopAnimation(storyboard, YubiKeyBorder, beginTimeMs: 1150);
            AddIconPopAnimation(storyboard, KeyFileBorder, beginTimeMs: 1650);

            // Path fade-in

            // Last icon ends at ~2000 ms (1650 + 350), so the path begins right after
            var pathFade = new DoubleAnimation
            {
                From = 0d,
                To = 0.25d,
                Duration = new Duration(TimeSpan.FromMilliseconds(400)),
                BeginTime = TimeSpan.FromMilliseconds(2050), // slight buffer after last icon
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(pathFade, AuthenticationPath);
            Storyboard.SetTargetProperty(pathFade, "Opacity");
            storyboard.Children.Add(pathFade);

            // Awaitable completion
            var tcs = new TaskCompletionSource<object?>();
            storyboard.Completed += Storyboard_Completed;
            storyboard.Begin();

            await tcs.Task;
            return;

            void Storyboard_Completed(object? sender, object e)
            {
                storyboard.Completed -= Storyboard_Completed;
                tcs.SetResult(null);
            }
        }

        private static void AddIconPopAnimation(
            Storyboard storyboard,
            UIElement icon,
            double beginTimeMs)
        {
            var duration = TimeSpan.FromMilliseconds(350);
            var easing = new CubicEase { EasingMode = EasingMode.EaseOut };

            // Opacity fade-in
            var opacityAnim = new DoubleAnimation
            {
                From = 0d,
                To = 1d,
                Duration = new Duration(duration),
                BeginTime = TimeSpan.FromMilliseconds(beginTimeMs),
                EasingFunction = easing
            };
            Storyboard.SetTarget(opacityAnim, icon);
            Storyboard.SetTargetProperty(opacityAnim, "Opacity");
            storyboard.Children.Add(opacityAnim);

            // Scale X
            var scaleXAnim = new DoubleAnimation
            {
                From = 0.4d,
                To = 1d,
                Duration = new Duration(duration),
                BeginTime = TimeSpan.FromMilliseconds(beginTimeMs),
                EasingFunction = easing
            };
            Storyboard.SetTarget(scaleXAnim, icon);
            Storyboard.SetTargetProperty(scaleXAnim, "(UIElement.RenderTransform).(ScaleTransform.ScaleX)");
            storyboard.Children.Add(scaleXAnim);

            // Scale Y
            var scaleYAnim = new DoubleAnimation
            {
                From = 0.4d,
                To = 1d,
                Duration = new Duration(duration),
                BeginTime = TimeSpan.FromMilliseconds(beginTimeMs),
                EasingFunction = easing
            };
            Storyboard.SetTarget(scaleYAnim, icon);
            Storyboard.SetTargetProperty(scaleYAnim, "(UIElement.RenderTransform).(ScaleTransform.ScaleY)");
            storyboard.Children.Add(scaleYAnim);
        }
    }
}
