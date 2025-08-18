using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.UI.ViewModels.Authentication;

#if ANDROID
using SecureFolderFS.Maui.Platforms.Android.ViewModels;
using SecureFolderFS.Shared.Extensions;
#endif

namespace SecureFolderFS.Maui.TemplateSelectors
{
    internal sealed class LoginTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? PasswordTemplate { get; set; }

        public DataTemplate? KeyFileTemplate { get; set; }

        public DataTemplate? ErrorTemplate { get; set; }

        public DataTemplate? AndroidBiometricsTemplate { get; set; }

        /// <inheritdoc/>
        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        {
#if ANDROID
            AndroidBiometricsTemplate ??= App.Instance.Resources.Get("AndroidBiometricsLoginTemplate") as DataTemplate;
#endif

            return item switch
            {
                PasswordLoginViewModel => PasswordTemplate,
                KeyFileLoginViewModel => KeyFileTemplate,
#if ANDROID
                AndroidBiometricLoginViewModel => AndroidBiometricsTemplate,
#endif
                ErrorViewModel => ErrorTemplate,
                _ => null
            };
        }
    }
}
