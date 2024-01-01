using Microsoft.UI.Xaml;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets;

namespace SecureFolderFS.Uno.TemplateSelectors
{
    internal sealed class WidgetsTemplateSelector : GenericTemplateSelector<BaseWidgetViewModel>
    {
        public DataTemplate? HealthWidgetTemplate { get; set; }

        public DataTemplate? GraphsWidgetTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(BaseWidgetViewModel? item, DependencyObject container)
        {
            return item switch
            {
                VaultHealthWidgetViewModel => HealthWidgetTemplate,
                GraphsWidgetViewModel => GraphsWidgetTemplate,
                _ => base.SelectTemplateCore(item, container)
            };
        }
    }
}
