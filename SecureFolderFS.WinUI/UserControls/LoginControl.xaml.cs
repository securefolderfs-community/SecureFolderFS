using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.UserControls
{
    public sealed partial class LoginControl : UserControl
    {
        public LoginControl()
        {
            InitializeComponent();
        }

        private async void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            if (button.DataContext is not PasswordControl passwordControl)
                return;

            button.IsEnabled = false;
            await Task.Delay(5);

            button.Command?.Execute(passwordControl.GetPassword());

            await Task.Delay(5);
            button.IsEnabled = true;
        }

        public INotifyPropertyChanged LoginTypeViewModel
        {
            get => (INotifyPropertyChanged)GetValue(LoginTypeViewModelProperty);
            set => SetValue(LoginTypeViewModelProperty, value);
        }
        public static readonly DependencyProperty LoginTypeViewModelProperty =
            DependencyProperty.Register(nameof(LoginTypeViewModel), typeof(INotifyPropertyChanged), typeof(LoginControl), new PropertyMetadata(null));
    }
}
