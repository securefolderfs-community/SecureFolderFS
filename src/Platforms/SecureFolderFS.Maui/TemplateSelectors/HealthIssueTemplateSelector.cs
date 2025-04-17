using SecureFolderFS.UI.ViewModels.Health;

namespace SecureFolderFS.Maui.TemplateSelectors
{
    internal sealed class HealthIssueTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? IssueTemplate { get; set; }

        public DataTemplate? NameIssueTemplate { get; set; }

        public DataTemplate? FileDataIssueTemplate { get; set; }

        public DataTemplate? DirectoryIssueTemplate { get; set; }
        
        /// <inheritdoc/>
        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        {
            return item switch
            {
                HealthNameIssueViewModel => NameIssueTemplate,
                HealthFileDataIssueViewModel => FileDataIssueTemplate,
                HealthDirectoryIssueViewModel => DirectoryIssueTemplate,
                not null => IssueTemplate,
                _ => null
            };
        }
    }
}
