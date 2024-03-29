using System.ComponentModel;

namespace SecureFolderFS.Maui.UserControls
{
    public partial class LoginControl : ContentView
    {
        public LoginControl()
        {
            BindingContext = this;
            InitializeComponent();
        }

        public INotifyPropertyChanged? LoginTypeViewModel
        {
            get => (INotifyPropertyChanged?)GetValue(LoginTypeViewModelProperty);
            set => SetValue(LoginTypeViewModelProperty, value);
        }
        public static readonly BindableProperty LoginTypeViewModelProperty =
            BindableProperty.Create(nameof(LoginTypeViewModel), typeof(INotifyPropertyChanged), typeof(LoginControl), null);
    }
}
