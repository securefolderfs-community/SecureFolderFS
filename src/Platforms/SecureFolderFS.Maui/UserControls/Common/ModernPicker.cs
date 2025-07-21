namespace SecureFolderFS.Maui.UserControls.Common
{
    public class ModernPicker : Picker
    {
        public bool IsTransparent
        {
            get => (bool)GetValue(IsTransparentProperty);
            set => SetValue(IsTransparentProperty, value);
        }
        public static readonly BindableProperty IsTransparentProperty =
            BindableProperty.Create(nameof(IsTransparent), typeof(bool), typeof(ModernPicker), defaultValue: false);
    }
}
