using Microsoft.UI.Xaml;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Data;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Health;

namespace SecureFolderFS.Uno.TemplateSelectors
{
    internal sealed class WidgetsTemplateSelector : BaseTemplateSelector<BaseWidgetViewModel>
    {
        public DataTemplate? HealthWidgetTemplate { get; set; }

        public DataTemplate? GraphsWidgetTemplate { get; set; }

        public DataTemplate? AggregatedDataWidgetTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(BaseWidgetViewModel? item, DependencyObject container)
        {
            return item switch
            {
                HealthWidgetViewModel => HealthWidgetTemplate,
                GraphsWidgetViewModel => GraphsWidgetTemplate,
                AggregatedDataWidgetViewModel => AggregatedDataWidgetTemplate,
                _ => base.SelectTemplateCore(item, container)
            };
        }
    }
}
