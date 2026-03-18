using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.UI.ViewModels.Authentication;
using SecureFolderFS.Uno.ViewModels.DeviceLink;
using SecureFolderFS.Uno.ViewModels.WindowsHello;
using SecureFolderFS.Uno.ViewModels.YubiKey;
#if __UNO_SKIA_MACOS__
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Uno.Platforms.Desktop.ViewModels;
#endif

namespace SecureFolderFS.Uno.TemplateSelectors
{
    internal sealed class LoginTemplateSelector : BaseTemplateSelector<ObservableObject>
    {
        public DataTemplate? PasswordTemplate { get; set; }

        public DataTemplate? KeyFileTemplate { get; set; }

        public DataTemplate? WindowsHelloTemplate { get; set; }

        public DataTemplate? YubiKeyTemplate { get; set; }

        public DataTemplate? TouchIDTemplate { get; set; }

        public DataTemplate? DeviceLinkTemplate { get; set; }

        public DataTemplate? MigrationTemplate { get; set; }

        public DataTemplate? ErrorTemplate { get; set; }

        public DataTemplate? UnsupportedTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(ObservableObject? item, DependencyObject container)
        {
#if __UNO_SKIA_MACOS__
            TouchIDTemplate ??= App.Instance?.Resources.Get("TouchIDRegisterTemplate") as DataTemplate;            
#endif

            return item switch
            {
                PasswordLoginViewModel => PasswordTemplate,
                KeyFileLoginViewModel => KeyFileTemplate,
                WindowsHelloLoginViewModel => WindowsHelloTemplate,
                YubiKeyLoginViewModel => YubiKeyTemplate,
#if __UNO_SKIA_MACOS__
                MacOSBiometricsLoginViewModel => TouchIDTemplate,
#endif
                DeviceLinkLoginViewModel => DeviceLinkTemplate,
                MigrationViewModel => MigrationTemplate,
                ErrorViewModel => ErrorTemplate,
                UnsupportedViewModel => UnsupportedTemplate,
                _ => base.SelectTemplateCore(item, container)
            };
        }
    }
}
