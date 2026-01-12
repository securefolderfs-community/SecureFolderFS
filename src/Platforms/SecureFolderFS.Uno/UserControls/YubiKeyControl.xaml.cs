using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Shared.Extensions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls
{
    public sealed partial class YubiKeyControl : UserControl
    {
        public YubiKeyControl()
        {
            InitializeComponent();
        }

        private void YubiKeyLight_Loaded(object sender, RoutedEventArgs e)
        {
            // Start blinking if IsBlinking is already true when loaded
            if (IsBlinking)
                YubiKeyBlinkStoryboard?.Begin();
        }

        private void UpdateBlinkingState(bool isBlinking)
        {
            App.Instance?.MainWindowSynchronizationContext.PostOrExecute(_ =>
            {
                if (isBlinking)
                {
                    YubiKeyLight.Visibility = Visibility.Visible;
                    YubiKeyBlinkStoryboard?.Begin();
                }
                else
                {
                    YubiKeyBlinkStoryboard?.Stop();
                    YubiKeyLight.Visibility = Visibility.Collapsed;
                }
            });
        }

        public bool IsBlinking
        {
            get => (bool)GetValue(IsBlinkingProperty);
            set => SetValue(IsBlinkingProperty, value);
        }
        public static readonly DependencyProperty IsBlinkingProperty =
            DependencyProperty.Register(nameof(IsBlinking), typeof(bool), typeof(YubiKeyControl), new PropertyMetadata(false,
                static (s, e) =>
                {
                    if (s is not YubiKeyControl yubiKeyControl)
                        return;
                    
                    if (e.NewValue is not bool isBlinking)
                        return;
                    
                    yubiKeyControl.UpdateBlinkingState(isBlinking);
                }));
    }
}
