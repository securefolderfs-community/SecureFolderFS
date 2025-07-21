using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;

namespace SecureFolderFS.Maui.TemplateSelectors
{
    internal sealed class BrowserViewItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? ListItemTemplate { get; set; }

        public DataTemplate? GridItemTemplate { get; set; }

        public DataTemplate? GalleryItemTemplate { get; set; }

        /// <inheritdoc/>
        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        {
            if (item is not BrowserItemViewModel browserItem)
                return null;

            return browserItem.BrowserViewModel.Layouts.BrowserViewType switch
            {
                BrowserViewType.ListView or BrowserViewType.ColumnView => ListItemTemplate,
                BrowserViewType.SmallGridView or BrowserViewType.MediumGridView or BrowserViewType.LargeGridView => GridItemTemplate,
                BrowserViewType.SmallGalleryView or BrowserViewType.MediumGalleryView or BrowserViewType.LargeGalleryView => GalleryItemTemplate,
                _ => null
            };
        }
    }
}
