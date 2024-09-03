using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Categories;

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
