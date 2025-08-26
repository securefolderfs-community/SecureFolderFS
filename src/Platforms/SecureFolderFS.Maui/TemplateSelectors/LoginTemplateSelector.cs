using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.UI.ViewModels.Authentication;
using SecureFolderFS.Shared.Extensions;

#if ANDROID
using SecureFolderFS.Maui.Platforms.Android.ViewModels;
#elif IOS
using SecureFolderFS.Maui.Platforms.iOS.ViewModels;
#endif

namespace SecureFolderFS.Maui.TemplateSelectors
{
    internal sealed class LoginTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? PasswordTemplate { get; set; }

        public DataTemplate? KeyFileTemplate { get; set; }

        public DataTemplate? ErrorTemplate { get; set; }

#if ANDROID
        public DataTemplate? AndroidBiometricsTemplate { get; set; }
#elif IOS
        public DataTemplate? IOSBiometricsTemplate { get; set; }
#endif

        /// <inheritdoc/>
        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        {
#if ANDROID
            AndroidBiometricsTemplate ??= App.Instance.Resources.Get("AndroidBiometricsLoginTemplate") as DataTemplate;
#elif IOS
            IOSBiometricsTemplate ??= App.Instance.Resources.Get("IOSBiometricsLoginTemplate") as DataTemplate;
#endif

            return item switch
            {
                PasswordLoginViewModel => PasswordTemplate,
                KeyFileLoginViewModel => KeyFileTemplate,
#if ANDROID
                AndroidBiometricLoginViewModel => AndroidBiometricsTemplate,
#elif IOS
                IOSBiometricLoginViewModel => IOSBiometricsTemplate,
#endif
                ErrorViewModel => ErrorTemplate,
                _ => null
            };
        }
    }
}
