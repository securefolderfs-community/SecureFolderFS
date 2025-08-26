using SecureFolderFS.UI.ViewModels.Authentication;
using SecureFolderFS.Shared.Extensions;

#if ANDROID
using SecureFolderFS.Maui.Platforms.Android.ViewModels;
#elif IOS
using SecureFolderFS.Maui.Platforms.iOS.ViewModels;
#endif

namespace SecureFolderFS.Maui.TemplateSelectors
{
    internal sealed class RegistrationTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? PasswordTemplate { get; set; }

        public DataTemplate? KeyFileTemplate { get; set; }

#if ANDROID
        public DataTemplate? AndroidBiometricsTemplate { get; set; }
#elif IOS
        public DataTemplate? IOSBiometricsTemplate { get; set; }
#endif

        /// <inheritdoc/>
        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        {
#if ANDROID
            AndroidBiometricsTemplate ??= App.Instance.Resources.Get("AndroidBiometricsRegisterTemplate") as DataTemplate;
#elif IOS
            IOSBiometricsTemplate ??= App.Instance.Resources.Get("IOSBiometricsRegisterTemplate") as DataTemplate;
#endif

            return item switch
            {
                PasswordCreationViewModel => PasswordTemplate,
                KeyFileCreationViewModel => KeyFileTemplate,
#if ANDROID
                AndroidBiometricCreationViewModel => AndroidBiometricsTemplate,
#elif IOS
                IOSBiometricCreationViewModel => IOSBiometricsTemplate,
#endif
                _ => null
            };
        }
    }
}
