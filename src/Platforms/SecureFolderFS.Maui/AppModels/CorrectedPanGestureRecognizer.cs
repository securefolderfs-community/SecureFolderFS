// Some parts of the following code were used from https://github.com/dotnet/maui/issues/20772#issuecomment-2030914069

using Microsoft.Maui.Platform;

namespace SecureFolderFS.Maui.AppModels
{
    internal sealed class CorrectedPanGestureRecognizer : PanGestureRecognizer, IPanGestureController
    {
#if ANDROID
        // Index 0 is X, index 1 is Y
        private readonly int[] startingLocation = new int[2];
        private readonly int[] currentLocation = new int[2];
#endif

        void IPanGestureController.SendPan(Element sender, double totalX, double totalY, int gestureId)
        {
#if ANDROID
            ArgumentNullException.ThrowIfNull(sender.Handler.MauiContext?.Context);
            Android.Views.View view = sender.ToPlatform(sender.Handler.MauiContext);
            view.GetLocationOnScreen(currentLocation);
            totalX += sender.Handler.MauiContext.Context.FromPixels(currentLocation[0] - startingLocation[0]);
            totalY += sender.Handler.MauiContext.Context.FromPixels(currentLocation[1] - startingLocation[1]);

#endif
            PanUpdated?.Invoke(sender, new PanUpdatedEventArgs(GestureStatus.Running, gestureId, totalX, totalY));
        }

        void IPanGestureController.SendPanCanceled(Element sender, int gestureId)
        {
            PanUpdated?.Invoke(sender, new PanUpdatedEventArgs(GestureStatus.Canceled, gestureId));
        }

        void IPanGestureController.SendPanCompleted(Element sender, int gestureId)
        {
            PanUpdated?.Invoke(sender, new PanUpdatedEventArgs(GestureStatus.Completed, gestureId));
        }

        void IPanGestureController.SendPanStarted(Element sender, int gestureId)
        {
#if ANDROID
            ArgumentNullException.ThrowIfNull(sender.Handler.MauiContext);
            Android.Views.View view = sender.ToPlatform(sender.Handler.MauiContext);
            view.GetLocationOnScreen(startingLocation);
#endif
            PanUpdated?.Invoke(sender, new PanUpdatedEventArgs(GestureStatus.Started, gestureId));
        }

        public new event EventHandler<PanUpdatedEventArgs>? PanUpdated;
    }

}
