using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls
{
    [INotifyPropertyChanged]
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

        public INotifyPropertyChanged? CurrentViewModel
        {
            get => (INotifyPropertyChanged?)GetValue(CurrentViewModelProperty);
            set => SetValue(CurrentViewModelProperty, value);
        }
        public static readonly DependencyProperty CurrentViewModelProperty =
            DependencyProperty.Register(nameof(CurrentViewModel), typeof(INotifyPropertyChanged), typeof(LoginControl), new PropertyMetadata(defaultValue: null,
                (s, e) =>
                {
                    if (s is not LoginControl loginControl)
                        return;

                    loginControl.OnPropertyChanged(nameof(ProvideContinuationButton));
                }));

        public bool ProvideContinuationButton
        {
            get => (bool)GetValue(ProvideContinuationButtonProperty);
            set => SetValue(ProvideContinuationButtonProperty, value);
        }
        public static readonly DependencyProperty ProvideContinuationButtonProperty =
            DependencyProperty.Register(nameof(ProvideContinuationButton), typeof(bool), typeof(LoginControl), new PropertyMetadata(true));

        public bool IsKeyboardEventTrackingEnabled
        {
            get => (bool)GetValue(IsKeyboardEventTrackingEnabledProperty);
            set => SetValue(IsKeyboardEventTrackingEnabledProperty, value);
        }
        public static readonly DependencyProperty IsKeyboardEventTrackingEnabledProperty =
            DependencyProperty.Register(nameof(IsKeyboardEventTrackingEnabled), typeof(bool), typeof(LoginControl), new PropertyMetadata(true));
    }
}
