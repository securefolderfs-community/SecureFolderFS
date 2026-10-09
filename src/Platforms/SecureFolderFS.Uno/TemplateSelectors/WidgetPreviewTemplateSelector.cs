using Microsoft.UI.Xaml;
using SecureFolderFS.Sdk;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets;

namespace SecureFolderFS.Uno.TemplateSelectors
{
    internal sealed class WidgetPreviewTemplateSelector : BaseTemplateSelector<AvailableWidgetViewModel>
    {
        public DataTemplate? HealthPreviewTemplate { get; set; }

        public DataTemplate? GraphsPreviewTemplate { get; set; }

        public DataTemplate? AggregatedDataPreviewTemplate { get; set; }

        public DataTemplate? DropZonePreviewTemplate { get; set; }

        public DataTemplate? JournalPreviewTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(AvailableWidgetViewModel? item, DependencyObject container)
        {
            return item?.WidgetId switch
            {
                Constants.Widgets.HEALTH_WIDGET_ID => HealthPreviewTemplate,
                Constants.Widgets.GRAPHS_WIDGET_ID => GraphsPreviewTemplate,
                Constants.Widgets.AGGREGATED_DATA_WIDGET_ID => AggregatedDataPreviewTemplate,
                Constants.Widgets.DROP_ZONE_WIDGET_ID => DropZonePreviewTemplate,
                Constants.Widgets.JOURNAL_WIDGET_ID => JournalPreviewTemplate,
                _ => base.SelectTemplateCore(item, container)
            };
        }
    }
}
