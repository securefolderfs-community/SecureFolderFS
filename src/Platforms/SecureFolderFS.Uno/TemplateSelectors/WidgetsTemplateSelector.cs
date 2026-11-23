using Microsoft.UI.Xaml;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Data;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.DropZone;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Health;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Journal;

namespace SecureFolderFS.Uno.TemplateSelectors
{
    internal sealed class WidgetsTemplateSelector : BaseTemplateSelector<BaseWidgetViewModel>
    {
        public DataTemplate? HealthWidgetTemplate { get; set; }

        public DataTemplate? GraphsWidgetTemplate { get; set; }

        public DataTemplate? AggregatedDataWidgetTemplate { get; set; }

        public DataTemplate? DropZoneWidgetTemplate { get; set; }

        public DataTemplate? JournalWidgetTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(BaseWidgetViewModel? item, DependencyObject container)
        {
            return item switch
            {
                HealthWidgetViewModel => HealthWidgetTemplate,
                GraphsWidgetViewModel => GraphsWidgetTemplate,
                AggregatedDataWidgetViewModel => AggregatedDataWidgetTemplate,
                DropZoneWidgetViewModel => DropZoneWidgetTemplate,
                JournalWidgetViewModel => JournalWidgetTemplate,
                _ => base.SelectTemplateCore(item, container)
            };
        }
    }
}
