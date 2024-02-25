using Microsoft.UI.Xaml;
using SecureFolderFS.Uno.Enums;

namespace SecureFolderFS.Uno.TemplateSelectors
{
    internal sealed class ActionBlockModeTemplateSelector : BaseTemplateSelector<ActionBlockMode>
    {
        public DataTemplate? DefaultTemplate { get; set; }

        public DataTemplate? ClickableTemplate { get; set; }

        public DataTemplate? ExpandableTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(ActionBlockMode item, DependencyObject container)
        {
            return item switch
            {
                ActionBlockMode.Clickable => ClickableTemplate,
                ActionBlockMode.Expandable => ExpandableTemplate,
                _ => DefaultTemplate
            };
        }
    }
}
