using Microsoft.UI.Xaml;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Health;
using SecureFolderFS.UI.ViewModels.Health;

namespace SecureFolderFS.Uno.TemplateSelectors
{
    internal sealed class HealthIssueTemplateSelector : BaseTemplateSelector<HealthIssueViewModel>
    {
        public DataTemplate? IssueTemplate { get; set; }

        public DataTemplate? NameIssueTemplate { get; set; }

        public DataTemplate? FileDataIssueTemplate { get; set; }

        public DataTemplate? DirectoryIssueTemplate { get; set; }

        /// <inheritdoc/>
        protected override DataTemplate? SelectTemplateCore(HealthIssueViewModel? item, DependencyObject container)
        {
            return item switch
            {
                HealthNameIssueViewModel => NameIssueTemplate,
                HealthFileDataIssueViewModel => FileDataIssueTemplate,
                HealthDirectoryIssueViewModel => DirectoryIssueTemplate,
                not null => IssueTemplate,
                _ => base.SelectTemplateCore(item, container)
            };
        }
    }
}
