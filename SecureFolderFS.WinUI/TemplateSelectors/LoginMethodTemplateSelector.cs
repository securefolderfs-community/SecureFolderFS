using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using SecureFolderFS.Sdk.ViewModels.Vault.LoginStrategy;

namespace SecureFolderFS.WinUI.TemplateSelectors
{
    internal sealed class LoginMethodTemplateSelector : GenericTemplateSelector<ObservableObject>
    {
        public DataTemplate? AuthenticateTemplate { get; set; }

        public DataTemplate? LoginTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(ObservableObject? item, DependencyObject container)
        {
            if (item is LoginKeystoreSelectionViewModel)
            {
                return AuthenticateTemplate;
            }
            else if (item is LoginCredentialsViewModel)
            {
                return LoginTemplate;
            }

            return base.SelectTemplateCore(item, container);
        }
    }
}
