using SecureFolderFS.UI.ViewModels.Authentication;

#if ANDROID
using SecureFolderFS.Maui.Platforms.Android.ViewModels;
using SecureFolderFS.Shared.Extensions;
#endif

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
#if ANDROID
            AndroidBiometricsTemplate ??= App.Instance.Resources.Get("AndroidBiometricsRegisterTemplate") as DataTemplate;
#endif
            
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
