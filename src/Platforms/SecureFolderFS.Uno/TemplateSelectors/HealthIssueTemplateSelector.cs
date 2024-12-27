using Microsoft.UI.Xaml;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Health;
using SecureFolderFS.UI.ViewModels.Health;

namespace SecureFolderFS.Uno.TemplateSelectors
{
    internal sealed class HealthIssueTemplateSelector : BaseTemplateSelector<HealthIssueViewModel>
    {
        public DataTemplate? IssueTemplate { get; set; }

        public DataTemplate? RenameIssueTemplate { get; set; }

        public DataTemplate? CascadingIssuesTemplate { get; set; }

        /// <inheritdoc/>
        protected override DataTemplate? SelectTemplateCore(HealthIssueViewModel? item, DependencyObject container)
        {
            return item switch
            {
                HealthRenameIssueViewModel => RenameIssueTemplate,
                HealthCascadingIssuesViewModel => CascadingIssuesTemplate,
                { } => IssueTemplate,
                _ => base.SelectTemplateCore(item, container)
            };
        }
    }
}
