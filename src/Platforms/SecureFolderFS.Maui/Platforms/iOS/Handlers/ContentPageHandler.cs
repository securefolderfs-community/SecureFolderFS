using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using SecureFolderFS.Shared.Extensions;
using UIKit;

namespace SecureFolderFS.Maui.Handlers
{
    // Callsite only reachable on iOS 13 and above
    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    public sealed class ContentPageHandler : BaseContentPageHandler
    {
        protected override void ApplyHandler(IPlatformViewHandler viewHandler)
        {
            if (ThisPage is null)
                return;
            
            if (viewHandler.ViewController?.NavigationController?.NavigationBar is { } navigationBar)
                UpdateTitleMode(ThisPage, navigationBar);

            if (viewHandler.ViewController?.ParentViewController?.NavigationItem is { } navItem)
                UpdateToolbarItems(ThisPage, navItem);
        }

        private void UpdateTitleMode(ContentPage contentPage, UINavigationBar navigationBar)
        {
            var largeTitleMode = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetLargeTitleDisplay(contentPage);
            navigationBar.PrefersLargeTitles = largeTitleMode == LargeTitleDisplayMode.Always;
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
            
            static UIMenuElement CreateUIMenuElement(ToolbarItem item)
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
}
