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
    /// Stores the close action for each popup that is currently shown as an overlay.
    /// </summary>
    private static readonly Dictionary<Popup, Func<Task>> _closeActions = new();

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
        var originalContent = contentPage.Content;
        if (originalContent is null || popup.Content is null)
        {
            completionSource.SetResult();
            return completionSource.Task;
        }

        // If the root is already a Grid, inject directly into it to avoid reparenting
        // the existing content (which would reset view state like TransferControl visibility).
        // Otherwise, wrap in a new Grid.
        Grid rootGrid;
        bool injectedIntoExisting;

        if (originalContent is Grid existingGrid)
        {
            rootGrid = existingGrid;
            injectedIntoExisting = true;
        }
        else
        {
            rootGrid = new Grid();
            contentPage.Content = null;
            rootGrid.Children.Add(originalContent);
            contentPage.Content = rootGrid;
            injectedIntoExisting = false;
        }

        // Create the dimming background
        var dimBackground = new BoxView()
        {
            Color = MauiThemeHelper.Instance.ActualTheme == ThemeType.Light ? Colors.Black : Colors.DimGray,
            Opacity = 0,
            InputTransparent = false
        };
        // Span all rows/columns in case rootGrid has row/column definitions
        Grid.SetRowSpan(dimBackground, 99);
        Grid.SetColumnSpan(dimBackground, 99);

        // Create popup container
        var popupContainer = new Grid()
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center,
            InputTransparent = false,
            CascadeInputTransparent = false,
            Opacity = 0
        };
        Grid.SetRowSpan(popupContainer, 99);
        Grid.SetColumnSpan(popupContainer, 99);

        popup.HorizontalOptions = LayoutOptions.Fill;
        popup.VerticalOptions = LayoutOptions.Center;
        popup.CascadeInputTransparent = false;
        popup.InputTransparent = false;
        popup.Content.InputTransparent = false;
        popupContainer.Children.Add(popup);

        rootGrid.Children.Add(dimBackground);
        rootGrid.Children.Add(popupContainer);

        var isClosing = false;
        TapGestureRecognizer? tapGesture = null;
        EventHandler<TappedEventArgs>? tappedHandler = null;
        var originalBackButtonBehavior = Shell.GetBackButtonBehavior(contentPage);

        async Task CloseOverlayAsync()
        {
            if (isClosing)
                return;

            isClosing = true;
            _closeActions.Remove(popup);

            if (tapGesture is not null)
            {
                if (tappedHandler is not null)
                    tapGesture.Tapped -= tappedHandler;
                dimBackground.GestureRecognizers.Remove(tapGesture);
            }

            Shell.SetBackButtonBehavior(contentPage, originalBackButtonBehavior);

            await Task.WhenAll(
                dimBackground.FadeToAsync(0, FadeAnimationDuration, Easing.CubicOut),
                popupContainer.FadeToAsync(0, FadeAnimationDuration, Easing.CubicOut)
            );

            // Only remove the overlay layers — never touch the original content
            rootGrid.Children.Remove(dimBackground);
            rootGrid.Children.Remove(popupContainer);

            // If we wrapped in a new grid, unwrap
            if (!injectedIntoExisting)
            {
                contentPage.Content = null;
                rootGrid.Children.Remove(originalContent);
                contentPage.Content = originalContent;
            }

            completionSource.TrySetResult();
        }

        _closeActions[popup] = CloseOverlayAsync;
        if (dismissOnBackgroundTap)
        {
            tapGesture = new TapGestureRecognizer();
            tappedHandler = (_, _) => _ = CloseOverlayAsync();
            tapGesture.Tapped += tappedHandler;
            dimBackground.GestureRecognizers.Add(tapGesture);
        }

        Shell.SetBackButtonBehavior(contentPage, new BackButtonBehavior()
        {
            Command = new Command(() => _ = CloseOverlayAsync())
        });

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

    /// <summary>
    /// Closes a popup that was shown using <see cref="OverlayPopupAsync(Popup, bool)"/>.
    /// </summary>
    /// <param name="popup">The popup to close.</param>
    /// <returns>A task that completes when the popup has been closed.</returns>
    public static Task CloseOverlayAsync(this Popup popup)
    {
        if (_closeActions.TryGetValue(popup, out var closeAction))
            return closeAction();
        
        return Task.CompletedTask;
    }
}
