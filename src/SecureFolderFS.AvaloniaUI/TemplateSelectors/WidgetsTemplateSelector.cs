using Avalonia.Markup.Xaml.Templates;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets;

namespace SecureFolderFS.AvaloniaUI.TemplateSelectors
{
    internal sealed class WidgetsTemplateSelector : GenericTemplateSelector<BaseWidgetViewModel>
    {
        public DataTemplate? HealthWidgetTemplate { get; set; }

        public DataTemplate? GraphsWidgetTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(BaseWidgetViewModel? item)
        {
            return item switch
            {
                VaultHealthWidgetViewModel => HealthWidgetTemplate,
                GraphsWidgetViewModel => GraphsWidgetTemplate,
                _ => null
            };
        }
    }
}
