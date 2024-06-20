using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.UI.ViewModels;
using SecureFolderFS.Uno.ViewModels;

namespace SecureFolderFS.Uno.TemplateSelectors
{
    internal sealed class LoginMethodTemplateSelector : BaseTemplateSelector<ObservableObject>
    {
        public DataTemplate? PasswordTemplate { get; set; }

        public DataTemplate? KeyFileTemplate { get; set; }

        public DataTemplate? WindowsHelloTemplate { get; set; }

        public DataTemplate? MigrationTemplate { get; set; }

        public DataTemplate? ErrorTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(ObservableObject? item, DependencyObject container)
        {
            return item switch
            {
                PasswordLoginViewModel => PasswordTemplate,
                KeyFileLoginViewModel => KeyFileTemplate,
                WindowsHelloLoginViewModel => WindowsHelloTemplate,
                MigrationViewModel => MigrationTemplate,
                ErrorViewModel => ErrorTemplate,
                _ => base.SelectTemplateCore(item, container)
            };
        }
    }
}
