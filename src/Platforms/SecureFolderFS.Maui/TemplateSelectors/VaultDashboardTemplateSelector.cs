using SecureFolderFS.Sdk.ViewModels.Views.Vault.Dashboard;

namespace SecureFolderFS.Maui.TemplateSelectors
{
    internal sealed class VaultDashboardTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? OverviewTemplate { get; set; }

        public DataTemplate? PropertiesTemplate { get; set; }

        /// <inheritdoc/>
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            return item switch
            {
                VaultOverviewPageViewModel => OverviewTemplate,
                VaultPropertiesPageViewModel => PropertiesTemplate,
            } ?? throw new ArgumentException("Invalid view model type specified.", nameof(item));
        }
    }
}
