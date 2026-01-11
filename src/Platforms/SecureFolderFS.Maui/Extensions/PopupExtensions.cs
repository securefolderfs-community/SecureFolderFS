using CommunityToolkit.Maui.Views;
using SecureFolderFS.Maui.Helpers;
using SecureFolderFS.UI.Enums;

namespace SecureFolderFS.Maui.Extensions;

/// <summary>
/// Provides extension methods for displaying popups as overlays on top of the current page.
/// </summary>
internal static class PopupExtensions
{
    private const uint FadeAnimationDuration = 300;
    private const double OverlayOpacity = 0.6;
    
    /// <summary>
    /// Shows a popup as an overlay on top of the current page content with a fade animation.
    /// </summary>
    /// <param name="page">The page to show the popup on.</param>
    /// <param name="popup">The popup to display.</param>
    /// <param name="dismissOnBackgroundTap">If true, tapping the background will dismiss the popup.</param>
    /// <returns>A task that completes when the popup is closed.</returns>
    public static Task OverlayPopupAsync(this Page page, Popup popup, bool dismissOnBackgroundTap = true)
    {
        if (page is not ContentPage contentPage)
            return Task.CompletedTask;

        var completionSource = new TaskCompletionSource();

        // Get the current content
        var originalContent = contentPage.Content;
        if (originalContent is null)
        {
            completionSource.SetResult();
            return completionSource.Task;
        }

        // Create the overlay grid that will hold both original content and popup
        var overlayGrid = new Grid();

        // Remove original content from page first
        contentPage.Content = null;

        // Add original content to overlay grid
        overlayGrid.Children.Add(originalContent);

        // Create the dimming background
        var dimBackground = new BoxView()
        {
            Color = MauiThemeHelper.Instance.CurrentTheme == ThemeType.Light ? Colors.Black : Colors.DimGray,
            Opacity = 0,
            InputTransparent = false
        };
        overlayGrid.Children.Add(dimBackground);

        // Get the popup's content and detach it from the popup
        if (popup.Content is null)
        {
            // Restore original content if popup has no content
            contentPage.Content = originalContent;
            completionSource.SetResult();
            return completionSource.Task;
        }

        // Create a container for the popup content with centering
        var popupContainer = new Grid()
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center,
            InputTransparent = false,
            CascadeInputTransparent = false,
            Opacity = 0
        };

        // Add the popup content to the container
        popup.HorizontalOptions = LayoutOptions.Fill;
        popup.VerticalOptions = LayoutOptions.Center;
        popup.CascadeInputTransparent = false;
        popup.InputTransparent = false;
        popup.Content.InputTransparent = false;

        popupContainer.Children.Add(popup);

        overlayGrid.Children.Add(popupContainer);

        // Set the overlay grid as page content
        contentPage.Content = overlayGrid;

        // Track if already closing to prevent double-close
        var isClosing = false;

        // Event handlers that we need to unhook later
        TapGestureRecognizer? tapGesture = null;
        EventHandler<TappedEventArgs>? tappedHandler = null;
        EventHandler? closedHandler = null;
        EventHandler<NavigatedFromEventArgs>? navigatedFromHandler = null;

        // Store the original back button behavior to restore later
        var originalBackButtonBehavior = Shell.GetBackButtonBehavior(contentPage);

        void CleanupOverlay()
        {
            if (isClosing)
                return;

            isClosing = true;

            // Unhook all event handlers
            if (tapGesture is not null)
            {
                if (tappedHandler is not null)
                    tapGesture.Tapped -= tappedHandler;

                dimBackground.GestureRecognizers.Remove(tapGesture);
            }

            if (closedHandler is not null)
                popup.Closed -= closedHandler;

            if (navigatedFromHandler is not null)
                contentPage.NavigatedFrom -= navigatedFromHandler;

            // Restore the original back button behavior
            Shell.SetBackButtonBehavior(contentPage, originalBackButtonBehavior);

            // Remove overlay and restore original content
            overlayGrid.Children.Clear();
            contentPage.Content = originalContent;

            completionSource.TrySetResult();
        }

        // Define the close action with animation
        async Task CloseOverlayAsync()
        {
            if (isClosing)
                return;

            isClosing = true;

            // Unhook all event handlers
            if (tapGesture is not null)
            {
                if (tappedHandler is not null)
                    tapGesture.Tapped -= tappedHandler;

                dimBackground.GestureRecognizers.Remove(tapGesture);
            }

            if (closedHandler is not null)
                popup.Closed -= closedHandler;

            if (navigatedFromHandler is not null)
                contentPage.NavigatedFrom -= navigatedFromHandler;

            // Restore the original back button behavior
            Shell.SetBackButtonBehavior(contentPage, originalBackButtonBehavior);

            // Fade out animations
            await Task.WhenAll(
                dimBackground.FadeToAsync(0, FadeAnimationDuration, Easing.CubicOut),
                popupContainer.FadeToAsync(0, FadeAnimationDuration, Easing.CubicOut)
            );

            // Remove overlay and restore original content
            overlayGrid.Children.Clear();
            contentPage.Content = originalContent;

            completionSource.TrySetResult();
        }

        // Handle background tap for light dismiss
        if (dismissOnBackgroundTap)
        {
            tapGesture = new TapGestureRecognizer();
            tappedHandler = (_, _) => _ = CloseOverlayAsync();
            tapGesture.Tapped += tappedHandler;
            dimBackground.GestureRecognizers.Add(tapGesture);
        }

        // Subscribe to the popup's Closed event
        closedHandler = (_, _) => _ = CloseOverlayAsync();
        popup.Closed += closedHandler;

        // Handle page navigation (e.g., user taps NavigationBar back button)
        navigatedFromHandler = (_, _) => CleanupOverlay();
        contentPage.NavigatedFrom += navigatedFromHandler;

        // Handle Android back button/gesture - close popup instead of navigating
        var backButtonBehavior = new BackButtonBehavior()
        {
            Command = new Command(() => _ = CloseOverlayAsync())
        };
        Shell.SetBackButtonBehavior(contentPage, backButtonBehavior);

        // Fade in animations
        _ = Task.WhenAll(
            dimBackground.FadeToAsync(OverlayOpacity, FadeAnimationDuration, Easing.CubicIn),
            popupContainer.FadeToAsync(1, FadeAnimationDuration, Easing.CubicIn)
        );

        return completionSource.Task;
    }

    /// <summary>
    /// Shows a popup as an overlay on the current Shell page with a fade animation.
    /// </summary>
    /// <param name="popup">The popup to display.</param>
    /// <param name="dismissOnBackgroundTap">If true, tapping the background will dismiss the popup.</param>
    /// <returns>A task that completes when the popup is closed.</returns>
    public static Task OverlayPopupAsync(this Popup popup, bool dismissOnBackgroundTap = true)
    {
        var currentPage = Shell.Current?.CurrentPage;
        if (currentPage is not ContentPage contentPage)
            return Task.CompletedTask;

        return contentPage.OverlayPopupAsync(popup, dismissOnBackgroundTap);
    }
}
