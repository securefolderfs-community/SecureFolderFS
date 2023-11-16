using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.ViewModels.Views.Vault.Strategy;

namespace SecureFolderFS.AvaloniaUI.TemplateSelectors
{
    internal sealed class LoginMethodTemplateSelector : GenericTemplateSelector<ObservableObject>
    {
        public DataTemplate? AuthenticateTemplate { get; set; }

        public DataTemplate? LoginTemplate { get; set; }

        public DataTemplate? InvalidTemplate { get; set; }

        protected override IDataTemplate? SelectTemplateCore(ObservableObject? item)
        {
            return item switch
            {
                LoginKeystoreViewModel => AuthenticateTemplate,
                LoginCredentialsViewModel => LoginTemplate,
                LoginErrorViewModel => InvalidTemplate,
                _ => null
            };
        }
    }
}