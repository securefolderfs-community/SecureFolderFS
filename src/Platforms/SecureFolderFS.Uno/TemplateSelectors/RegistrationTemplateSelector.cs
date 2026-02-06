using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using SecureFolderFS.UI.ViewModels.Authentication;
using SecureFolderFS.Uno.ViewModels.DeviceLink;
using SecureFolderFS.Uno.ViewModels.WindowsHello;
using SecureFolderFS.Uno.ViewModels.YubiKey;

namespace SecureFolderFS.Uno.TemplateSelectors
{
    internal sealed class RegistrationTemplateSelector : BaseTemplateSelector<ObservableObject>
    {
        public DataTemplate? PasswordTemplate { get; set; }

        public DataTemplate? KeyFileTemplate { get; set; }

        public DataTemplate? WindowsHelloTemplate { get; set; }

        public DataTemplate? YubiKeyTemplate { get; set; }
        
        public DataTemplate? DeviceLinkTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(ObservableObject? item, DependencyObject container)
        {
            return item switch
            {
                PasswordCreationViewModel => PasswordTemplate,
                KeyFileCreationViewModel => KeyFileTemplate,
                WindowsHelloCreationViewModel => WindowsHelloTemplate,
                YubiKeyCreationViewModel => YubiKeyTemplate,
                DeviceLinkCreationViewModel => DeviceLinkTemplate,
                _ => base.SelectTemplateCore(item, container)
            };
        }
    }
}
