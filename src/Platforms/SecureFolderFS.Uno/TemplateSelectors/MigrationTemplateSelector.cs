using Microsoft.UI.Xaml;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;

namespace SecureFolderFS.Uno.TemplateSelectors
{
    internal sealed class MigrationTemplateSelector : BaseTemplateSelector<MigrationViewModel>
    {
        public DataTemplate? MigratorV1_V2 { get; set; }

        public DataTemplate? MigratorV2_V3 { get; set; }

        /// <inheritdoc/>
        protected override DataTemplate? SelectTemplateCore(MigrationViewModel? item, DependencyObject container)
        {
            return item?.FormatVersion switch
            {
                Core.Constants.Vault.Versions.V1 => MigratorV1_V2,
                Core.Constants.Vault.Versions.V2 => MigratorV2_V3,
                _ => base.SelectTemplateCore(item, container)
            };
        }
    }
}
