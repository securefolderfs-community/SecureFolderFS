using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace SecureFolderFS.WinUI.TemplateSelectors
{
    /// <summary>
    /// Template selector wrapper for <see cref="DataTemplateSelector"/> with generics support.
    /// </summary>
    /// <typeparam name="TItem">The type of item.</typeparam>
    internal abstract class GenericTemplateSelector<TItem> : DataTemplateSelector
    {
        /// <inheritdoc/>
        protected sealed override DataTemplate? SelectTemplateCore(object item)
        {
            return SelectTemplateCore((TItem?)item);
        }

        /// <inheritdoc/>
        protected sealed override DataTemplate? SelectTemplateCore(object item, DependencyObject container)
        {
            return SelectTemplateCore((TItem?)item, container);
        }

        protected virtual DataTemplate? SelectTemplateCore(TItem? item)
        {
            return base.SelectTemplateCore(item);
        }

        protected virtual DataTemplate? SelectTemplateCore(TItem? item, DependencyObject container)
        {
            return base.SelectTemplateCore(item, container);
        }
    }
}
