using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using SecureFolderFS.Maui.AppModels;
using SecureFolderFS.Maui.Views;
using SecureFolderFS.Shared.Extensions;
using UIKit;

namespace SecureFolderFS.Maui.Handlers
{
    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    public sealed class ContentPageExHandler : BaseContentPageHandler
    {
        protected override void ApplyHandler(IPlatformViewHandler viewHandler)
        {
            if (ThisPage is not ContentPageExtended thisPageEx)
                return;

            if (thisPageEx.ExToolbarItems is INotifyCollectionChanged notifyCollectionChanged)
            {
                notifyCollectionChanged.CollectionChanged -= ToolbarItemsEx_CollectionChanged;
                notifyCollectionChanged.CollectionChanged += ToolbarItemsEx_CollectionChanged;
            }

            if (viewHandler.ViewController?.ParentViewController?.NavigationItem is { } navItem)
                UpdateToolbarItems(thisPageEx, navItem);
        }

        private void ToolbarItemsEx_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (ThisPage is not ContentPageExtended thisPageEx)
                return;

            if (ViewController?.ParentViewController?.NavigationItem is { } navItem)
                UpdateToolbarItems(thisPageEx, navItem);
        }

        private void UpdateToolbarItems(ContentPageExtended contentPage, UINavigationItem navigationItem)
        {
            // Clear if ToolbarItems are empty
            if (contentPage.ExToolbarItems.IsEmpty())
            {
                navigationItem.RightBarButtonItems = null;
                return;
            }

            // Don't do anything if there are some items in both platform view and ToolbarItems
            if (!contentPage.ExToolbarItems.IsEmpty() && !navigationItem.RightBarButtonItems.IsEmpty())
                return;

            var rightBarItems = new List<UIBarButtonItem>();

            // Get primary items
            foreach (var item in contentPage.ExToolbarItems)
            {
                if (item.Order != ToolbarItemOrder.Secondary)
                    rightBarItems.Add(item.ToUIBarButtonItem());
            }

            // Get secondary items
            var secondaryItems = contentPage.ExToolbarItems
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
            
            static UIMenuElement CreateUIMenuElement(ExMenuItemBase item)
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
