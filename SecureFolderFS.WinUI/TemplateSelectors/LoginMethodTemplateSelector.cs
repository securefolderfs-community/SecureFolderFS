using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using SecureFolderFS.Sdk.ViewModels.Vault.Login;

namespace SecureFolderFS.WinUI.TemplateSelectors
{
    internal sealed class LoginMethodTemplateSelector : GenericTemplateSelector<ObservableObject>
    {
        public DataTemplate? AuthenticateDataTemplate { get; set; }

        public DataTemplate? LoginDataTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(ObservableObject? item, DependencyObject container)
        {
            if (item is LoginKeystoreSelectionViewModel)
                return AuthenticateDataTemplate;
            else if (item is LoginCredentialsViewModel)
                return LoginDataTemplate;

            return base.SelectTemplateCore(item, container);
        }
    }
}
