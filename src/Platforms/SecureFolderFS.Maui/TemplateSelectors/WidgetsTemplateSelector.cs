using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Data;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Health;

namespace SecureFolderFS.Maui.TemplateSelectors
{
    internal sealed class WidgetsTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? HealthWidgetTemplate { get; set; }

        public DataTemplate? AggregatedDataWidgetTemplate { get; set; }
        
        /// <inheritdoc/>
        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        {
            return item switch
            {
                HealthWidgetViewModel => HealthWidgetTemplate,
                AggregatedDataWidgetViewModel => AggregatedDataWidgetTemplate,
                _ => null
            };
        }
    }
}
