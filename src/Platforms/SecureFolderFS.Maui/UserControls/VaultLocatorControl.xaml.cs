using System.Windows.Input;

namespace SecureFolderFS.Maui.UserControls
{
    public partial class VaultLocatorControl : ContentView
    {
        public VaultLocatorControl()
        {
            InitializeComponent();
        }
        
        public bool IsConnected
        {
            get => (bool)GetValue(IsConnectedProperty);
            set => SetValue(IsConnectedProperty, value);
        }
        public static readonly BindableProperty IsConnectedProperty =
            BindableProperty.Create(nameof(IsConnected), typeof(bool), typeof(VaultLocatorControl));
        
        public bool IsRunning
        {
            get => (bool)GetValue(IsRunningProperty);
            set => SetValue(IsRunningProperty, value);
        }
        public static readonly BindableProperty IsRunningProperty =
            BindableProperty.Create(nameof(IsRunning), typeof(bool), typeof(VaultLocatorControl));
        
        public ICommand? ConnectCommand
        {
            get => (ICommand?)GetValue(ConnectCommandProperty);
            set => SetValue(ConnectCommandProperty, value);
        }
        public static readonly BindableProperty ConnectCommandProperty =
            BindableProperty.Create(nameof(ConnectCommand), typeof(ICommand), typeof(VaultLocatorControl));
        
        public object? LoginView
        {
            get => (object?)GetValue(LoginViewProperty);
            set => SetValue(LoginViewProperty, value);
        }
        public static readonly BindableProperty LoginViewProperty =
            BindableProperty.Create(nameof(LoginView), typeof(object), typeof(VaultLocatorControl));
    }
}

