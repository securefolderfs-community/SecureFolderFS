using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Handlers;
using SecureFolderFS.Shared.Extensions;
using UIKit;
using ContentView = Microsoft.Maui.Platform.ContentView;

namespace SecureFolderFS.Maui.Handlers
{
    // Callsite only reachable on iOS 13 and above
    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    public sealed class MenuBarContentPageHandler : PageHandler
    {
        private ContentPage? ThisPage => VirtualView as ContentPage;
        
        protected override void ConnectHandler(ContentView platformView)
        {
            base.ConnectHandler(platformView);
            if (ThisPage is null)
                return;
            
            ThisPage.Loaded += ContentPage_Loaded;
            ThisPage.NavigatedTo += ContentPage_NavigatedTo;
            App.Instance.AppResumed += App_Resumed;
        }

        private async void ContentPage_Loaded(object? sender, EventArgs e)
        {
            // Await a small delay for the UI to load
            await Task.Delay(100);
            UpdateToolbarItems();
        }

        private void ContentPage_NavigatedTo(object? sender, NavigatedToEventArgs e)
        {
            UpdateToolbarItems();
        }
        
        private void App_Resumed(object? sender, EventArgs e)
        {
            // When app is resumed, ToolbarItems are re-added
            UpdateToolbarItems();
        }

        private void UpdateToolbarItems()
        {
            if (ThisPage is null)
                return;
            
            if (this is not IPlatformViewHandler pvh)
                return;
            
            if (pvh.ViewController?.ParentViewController?.NavigationItem is not { } navItem)
                return;

            UpdateToolbarItems(ThisPage, navItem);
        }

        private void UpdateToolbarItems(ContentPage contentPage, UINavigationItem navigationItem)
        {
            if (contentPage.ToolbarItems.IsEmpty())
                return;

            var rightBarItems = new List<UIBarButtonItem>();
            
            // Get primary items
            foreach (var item in contentPage.ToolbarItems)
            {
                if (item.Order != ToolbarItemOrder.Secondary)
                    rightBarItems.Add(item.ToUIBarButtonItem());
            }
            
            // Get secondary items
            var secondaryItems = contentPage.ToolbarItems
                .Where(x => x.Order == ToolbarItemOrder.Secondary)
                .Select(CreateUIMenuElement)
                .ToArray();

            if (!secondaryItems.IsEmpty())
            {
                // Create a popup UIMenu
                var menu = UIMenu.Create(string.Empty, null, UIMenuIdentifier.Edit, UIMenuOptions.DisplayInline, secondaryItems);
                
                // Set ellipsis icon image (can also use UIImage.ActionsImage for a filled ellipsis)
                var menuButton = new UIBarButtonItem(UIImage.FromBundle("cupertino_ellipsis.png"), menu);

                // Add to final bar items
                rightBarItems.Add(menuButton);
            }
            
            // Assign the navigation bar buttons
            navigationItem.RightBarButtonItems = rightBarItems.ToArray();
        }
        
        private static UIMenuElement CreateUIMenuElement(ToolbarItem item)
        {
            // Create a UIAction for each ToolbarItem
            var imagePath = item.IconImageSource?.ToString();
            var image = string.IsNullOrEmpty(imagePath) ? null : UIImage.FromBundle(imagePath);
            var action = UIAction.Create(item.Text, image, null, _ =>
            {
                item.Command?.Execute(item.CommandParameter);
            });

            return action;
        }
    }
}
