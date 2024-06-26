using System.ComponentModel;
using Microsoft.UI.Xaml;
using SecureFolderFS.Sdk.ViewModels.Controls;

namespace SecureFolderFS.Uno.TemplateSelectors
{
    internal sealed class CredentialsTemplateSelector : BaseTemplateSelector<INotifyPropertyChanged>
    {
        public DataTemplate? LoginTemplate { get; set; }

        public DataTemplate? SelectionTemplate { get; set; }

        /// <inheritdoc/>
        protected override DataTemplate? SelectTemplateCore(INotifyPropertyChanged? item, DependencyObject container)
        {
            return item switch
            {
                LoginViewModel => LoginTemplate,
                CredentialsViewModel => SelectionTemplate,
                _ => base.SelectTemplateCore(item, container)
            };
        }
    }
}
