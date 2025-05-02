using SecureFolderFS.Maui.Platforms.Android.ViewModels;
using SecureFolderFS.UI.ViewModels.Authentication;

namespace SecureFolderFS.Maui.TemplateSelectors
{
    internal sealed class RegistrationTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? PasswordTemplate { get; set; }

        public DataTemplate? KeyFileTemplate { get; set; }
        
        public DataTemplate? AndroidBiometricsTemplate { get; set; }

        /// <inheritdoc/>
        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        {
            return item switch
            {
                PasswordCreationViewModel => PasswordTemplate,
                KeyFileCreationViewModel => KeyFileTemplate,
#if ANDROID
                AndroidBiometricCreationViewModel => AndroidBiometricsTemplate,
#endif
                _ => null
            };
        }
    }
}
