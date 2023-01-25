using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using FluentAvalonia.UI.Controls;

namespace SecureFolderFS.AvaloniaUI.TemplateSelectors
{
    /// <summary>
    /// Template selector wrapper for <see cref="DataTemplateSelector"/> with generics support.
    /// </summary>
    /// <typeparam name="TItem">The type of item.</typeparam>
    internal abstract class GenericTemplateSelector<TItem> : IDataTemplate
    {
        public IControl? Build(object? param)
        {
            if (param is not TItem item)
                throw new ArgumentException(nameof(param));

            var template = SelectTemplateCore(item);
            return template?.Build(item);
        }

        public bool Match(object? data)
        {
            if (data is not TItem item)
                return false;

            return SelectTemplateCore(item) != null;
        }

        protected abstract IDataTemplate? SelectTemplateCore(TItem? item);
    }
}