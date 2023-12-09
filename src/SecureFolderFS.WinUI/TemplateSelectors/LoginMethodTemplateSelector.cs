using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using SecureFolderFS.Sdk.ViewModels.Views;
using SecureFolderFS.Sdk.ViewModels.Views.Vault.Login;

namespace SecureFolderFS.WinUI.TemplateSelectors
{
    internal sealed class LoginMethodTemplateSelector : GenericTemplateSelector<ObservableObject>
    {
        public DataTemplate? AuthenticateTemplate { get; set; }

        public DataTemplate? PasswordTemplate { get; set; }

        public DataTemplate? MigrationTemplate { get; set; }

        public DataTemplate? ErrorTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(ObservableObject? item, DependencyObject container)
        {
            return item switch
            {
                AuthenticationViewModel => AuthenticateTemplate,
                PasswordViewModel => PasswordTemplate,
                MigrationViewModel => MigrationTemplate,
                ErrorViewModel => ErrorTemplate,
                _ => base.SelectTemplateCore(item, container)
            };
        }
    }
}
