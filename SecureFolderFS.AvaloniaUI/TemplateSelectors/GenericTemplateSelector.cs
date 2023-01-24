using Avalonia.Controls;
using Avalonia.Controls.Templates;
using FluentAvalonia.UI.Controls;

namespace SecureFolderFS.AvaloniaUI.TemplateSelectors
{
    /// <summary>
    /// Template selector wrapper for <see cref="DataTemplateSelector"/> with generics support.
    /// </summary>
    /// <typeparam name="TItem">The type of item.</typeparam>
    internal abstract class GenericTemplateSelector<TItem> : DataTemplateSelector
    {
        protected sealed override IDataTemplate? SelectTemplateCore(object item)
        {
            return base.SelectTemplateCore(item);
        }

        protected sealed override IDataTemplate? SelectTemplateCore(object item, IControl container)
        {
            return base.SelectTemplateCore(item, container);
        }

        protected virtual IDataTemplate? SelectTemplateCore(TItem? item)
        {
            return base.SelectTemplateCore(item);
        }

        protected virtual IDataTemplate? SelectTemplateCore(TItem? item, IControl container)
        {
            return base.SelectTemplateCore(item, container);
        }
    }
}