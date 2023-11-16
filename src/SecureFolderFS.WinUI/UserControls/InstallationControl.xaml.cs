using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Windows.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.UserControls
{
    public sealed partial class InstallationControl : UserControl
    {
        public InstallationControl()
        {
            InitializeComponent();
        }

        public string? StatusText
        {
            get => (string?)GetValue(StatusTextProperty);
            set => SetValue(StatusTextProperty, value);
        }
        public static readonly DependencyProperty StatusTextProperty =
            DependencyProperty.Register(nameof(StatusText), typeof(string), typeof(InstallationControl), new PropertyMetadata(null));

        public ICommand? PauseResumeCommand
        {
            get => (ICommand?)GetValue(PauseResumeCommandProperty);
            set => SetValue(PauseResumeCommandProperty, value);
        }
        public static readonly DependencyProperty PauseResumeCommandProperty =
            DependencyProperty.Register(nameof(PauseResumeCommand), typeof(ICommand), typeof(InstallationControl), new PropertyMetadata(null));

        public ICommand? CancelCommand
        {
            get => (ICommand?)GetValue(CancelCommandProperty);
            set => SetValue(CancelCommandProperty, value);
        }
        public static readonly DependencyProperty CancelCommandProperty =
            DependencyProperty.Register(nameof(CancelCommand), typeof(ICommand), typeof(InstallationControl), new PropertyMetadata(null));

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(InstallationControl), new PropertyMetadata(0.0d));

        public bool IsPaused
        {
            get => (bool)GetValue(IsPausedProperty);
            set => SetValue(IsPausedProperty, value);
        }
        public static readonly DependencyProperty IsPausedProperty =
            DependencyProperty.Register(nameof(IsPaused), typeof(bool), typeof(InstallationControl), new PropertyMetadata(false));

        public bool IsIndeterminate
        {
            get => (bool)GetValue(IsIndeterminateProperty);
            set => SetValue(IsIndeterminateProperty, value);
        }
        public static readonly DependencyProperty IsIndeterminateProperty =
            DependencyProperty.Register(nameof(IsIndeterminate), typeof(bool), typeof(InstallationControl), new PropertyMetadata(false));
    }
}
