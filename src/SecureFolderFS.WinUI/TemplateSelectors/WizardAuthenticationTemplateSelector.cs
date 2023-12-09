using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard.NewVault.Signup;

namespace SecureFolderFS.WinUI.TemplateSelectors
{
    internal sealed class WizardAuthenticationTemplateSelector : GenericTemplateSelector<ObservableObject>
    {
        public DataTemplate? PasswordTemplate { get; set; }

        public DataTemplate? AuthenticationTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(ObservableObject? item, DependencyObject container)
        {
            return item switch
            {
                PasswordWizardViewModel => PasswordTemplate,
                AuthenticationWizardViewModel => AuthenticationTemplate,
                _ => base.SelectTemplateCore(item, container)
            };
        }
    }
}
