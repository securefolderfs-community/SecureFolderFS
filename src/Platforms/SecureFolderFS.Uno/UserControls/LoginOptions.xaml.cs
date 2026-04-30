using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls
{
    public sealed partial class LoginOptions : UserControl
    {
        public LoginOptions()
        {
            InitializeComponent();
        }

        private void LoginOptions_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }
        
        public bool ShouldSaveCredentials
        {
            get => (bool)GetValue(ShouldSaveCredentialsProperty);
            set => SetValue(ShouldSaveCredentialsProperty, value);
        }
        public static readonly DependencyProperty ShouldSaveCredentialsProperty =
            DependencyProperty.Register(nameof(ShouldSaveCredentials), typeof(bool), typeof(LoginOptions), new PropertyMetadata(false));
        
        public bool AreCredentialsSaved
        {
            get => (bool)GetValue(AreCredentialsSavedProperty);
            set => SetValue(AreCredentialsSavedProperty, value);
        }
        public static readonly DependencyProperty AreCredentialsSavedProperty =
            DependencyProperty.Register(nameof(AreCredentialsSaved), typeof(bool), typeof(LoginOptions), new PropertyMetadata(false));

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(LoginOptions), new PropertyMetadata(false));

        public bool IsLoginSequence
        {
            get => (bool)GetValue(IsLoginSequenceProperty);
            set => SetValue(IsLoginSequenceProperty, value);
        }
        public static readonly DependencyProperty IsLoginSequenceProperty =
            DependencyProperty.Register(nameof(IsLoginSequence), typeof(bool), typeof(LoginOptions), new PropertyMetadata(false));

        public ICommand? DiscardSavedCredentialsCommand
        {
            get => (ICommand?)GetValue(DiscardSavedCredentialsCommandProperty);
            set => SetValue(DiscardSavedCredentialsCommandProperty, value);
        }
        public static readonly DependencyProperty DiscardSavedCredentialsCommandProperty =
            DependencyProperty.Register(nameof(DiscardSavedCredentialsCommand), typeof(ICommand), typeof(LoginOptions), new PropertyMetadata(null));
        
        public ICommand? RestartLoginCommand
        {
            get => (ICommand?)GetValue(RestartLoginCommandProperty);
            set => SetValue(RestartLoginCommandProperty, value);
        }
        public static readonly DependencyProperty RestartLoginCommandProperty =
            DependencyProperty.Register(nameof(RestartLoginCommand), typeof(ICommand), typeof(LoginOptions), new PropertyMetadata(null));

        public ICommand? RecoverAccessCommand
        {
            get => (ICommand?)GetValue(RecoverAccessCommandProperty);
            set => SetValue(RecoverAccessCommandProperty, value);
        }
        public static readonly DependencyProperty RecoverAccessCommandProperty =
            DependencyProperty.Register(nameof(RecoverAccessCommand), typeof(ICommand), typeof(LoginOptions), new PropertyMetadata(null));
    }
}
