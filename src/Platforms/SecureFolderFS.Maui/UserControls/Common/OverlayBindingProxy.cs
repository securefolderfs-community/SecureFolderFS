using SecureFolderFS.Sdk.ViewModels.Views;

namespace SecureFolderFS.Maui.UserControls.Common
{
    public class OverlayBindingProxy : BindableObject
    {
        public IOverlayControls? DataBinding
        {
            get => (IOverlayControls?)GetValue(DataBindingProperty);
            set => SetValue(DataBindingProperty, value);
        }
        public static readonly BindableProperty DataBindingProperty =
            BindableProperty.Create(nameof(DataBinding), typeof(IOverlayControls), typeof(OverlayBindingProxy));
    }
}
