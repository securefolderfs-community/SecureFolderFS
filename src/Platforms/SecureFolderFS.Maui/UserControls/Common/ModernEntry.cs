namespace SecureFolderFS.Maui.UserControls.Common
{
    public class ModernEntry : Entry
    {
        public bool IsPadded
        {
            get => (bool)GetValue(IsPaddedProperty);
            set => SetValue(IsPaddedProperty, value);
        }
        public static readonly BindableProperty IsPaddedProperty =
            BindableProperty.Create(nameof(IsPadded), typeof(bool), typeof(ModernEntry), defaultValue: true);
    }
}
