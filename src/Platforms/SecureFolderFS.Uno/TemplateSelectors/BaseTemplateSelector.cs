using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace SecureFolderFS.Uno.TemplateSelectors
{
    /// <summary>
    /// A generic template selector wrapper for <see cref="DataTemplateSelector"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of item.</typeparam>
    internal abstract class BaseTemplateSelector<TItem> : DataTemplateSelector
    {
        /// <inheritdoc/>
        protected sealed override DataTemplate? SelectTemplateCore(object item)
        {
            if (item is not TItem typedItem)
                return SelectTemplateCore(default);

            return SelectTemplateCore(typedItem);
        }

        /// <inheritdoc/>
        protected sealed override DataTemplate? SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is not TItem typedItem)
                return SelectTemplateCore(default, container);

            return SelectTemplateCore(typedItem, container);
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
