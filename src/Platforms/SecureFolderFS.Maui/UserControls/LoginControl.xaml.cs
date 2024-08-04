using System.ComponentModel;

namespace SecureFolderFS.Maui.UserControls
{
    public partial class LoginControl : ContentView
    {
        public LoginControl()
        {
            InitializeComponent();
            RootGrid.BindingContext = this;
        }

        public INotifyPropertyChanged? CurrentViewModel
        {
            get => (INotifyPropertyChanged?)GetValue(CurrentViewModelProperty);
            set => SetValue(CurrentViewModelProperty, value);
        }
        public static readonly BindableProperty CurrentViewModelProperty =
            BindableProperty.Create(nameof(CurrentViewModel), typeof(INotifyPropertyChanged), typeof(LoginControl), null);
    }
}
