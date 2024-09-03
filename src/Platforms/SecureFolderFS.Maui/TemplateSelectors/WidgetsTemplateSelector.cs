namespace SecureFolderFS.Maui.TemplateSelectors
{
    internal sealed class WidgetsTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? HealthWidgetTemplate { get; set; }

        public DataTemplate? AggregatedDataWidgetTemplate { get; set; }
        
        /// <inheritdoc/>
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            throw new NotImplementedException();
        }
    }
}
