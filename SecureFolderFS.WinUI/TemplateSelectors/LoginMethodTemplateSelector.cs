using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using SecureFolderFS.Sdk.ViewModels.Vault.LoginStrategy;
using SecureFolderFS.Sdk.ViewModels.Views.Vault.Strategy;

namespace SecureFolderFS.WinUI.TemplateSelectors
{
    internal sealed class LoginMethodTemplateSelector : GenericTemplateSelector<ObservableObject>
    {
        public DataTemplate? AuthenticateTemplate { get; set; }

        public DataTemplate? LoginTemplate { get; set; }

        public DataTemplate? InvalidTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(ObservableObject? item, DependencyObject container)
        {
            return item switch
            {
                LoginKeystoreViewModel => AuthenticateTemplate,
                LoginCredentialsViewModel => LoginTemplate,
                LoginErrorViewModel => InvalidTemplate,
                _ => base.SelectTemplateCore(item, container)
            };
        }
    }
}
