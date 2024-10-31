using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Enums;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls.Widgets
{
    public sealed partial class HealthWidget : UserControl
    {
        public HealthWidget()
        {
            InitializeComponent();
        }

        public SeverityType Severity
        {
            get => (SeverityType)GetValue(SeverityProperty);
            set => SetValue(SeverityProperty, value);
        }
        public static readonly DependencyProperty SeverityProperty =
            DependencyProperty.Register(nameof(Severity), typeof(SeverityType), typeof(HealthWidget), new PropertyMetadata(SeverityType.Default));

        public string? StatusTitle
        {
            get => (string?)GetValue(StatusTitleProperty);
            set => SetValue(StatusTitleProperty, value);
        }
        public static readonly DependencyProperty StatusTitleProperty =
            DependencyProperty.Register(nameof(StatusTitle), typeof(string), typeof(HealthWidget), new PropertyMetadata(null));

        public string? LastCheckedText
        {
            get => (string?)GetValue(LastCheckedTextProperty);
            set => SetValue(LastCheckedTextProperty, value);
        }
        public static readonly DependencyProperty LastCheckedTextProperty =
            DependencyProperty.Register(nameof(LastCheckedText), typeof(string), typeof(HealthWidget), new PropertyMetadata(null));

        public ICommand? StartScanningCommand
        {
            get => (ICommand)GetValue(StartScanningCommandProperty);
            set => SetValue(StartScanningCommandProperty, value);
        }
        public static readonly DependencyProperty StartScanningCommandProperty =
            DependencyProperty.Register(nameof(StartScanningCommand), typeof(ICommand), typeof(HealthWidget), new PropertyMetadata(defaultValue: null));

        public ICommand? OpenVaultHealthCommand
        {
            get => (ICommand?)GetValue(OpenVaultHealthCommandProperty);
            set => SetValue(OpenVaultHealthCommandProperty, value);
        }
        public static readonly DependencyProperty OpenVaultHealthCommandProperty =
            DependencyProperty.Register(nameof(OpenVaultHealthCommand), typeof(ICommand), typeof(HealthWidget), new PropertyMetadata(defaultValue: null));

        public ICommand? CancelCommand
        {
            get => (ICommand?)GetValue(CancelCommandProperty);
            set => SetValue(CancelCommandProperty, value);
        }
        public static readonly DependencyProperty CancelCommandProperty =
            DependencyProperty.Register(nameof(CancelCommand), typeof(ICommand), typeof(HealthWidget), new PropertyMetadata(null));

        public bool IsProgressing
        {
            get => (bool)GetValue(IsProgressingProperty);
            set => SetValue(IsProgressingProperty, value);
        }
        public static readonly DependencyProperty IsProgressingProperty =
            DependencyProperty.Register(nameof(IsProgressing), typeof(bool), typeof(HealthWidget), new PropertyMetadata(false));

        public double CurrentProgress
        {
            get => (double)GetValue(CurrentProgressProperty);
            set => SetValue(CurrentProgressProperty, value);
        }
        public static readonly DependencyProperty CurrentProgressProperty =
            DependencyProperty.Register(nameof(CurrentProgress), typeof(double), typeof(HealthWidget), new PropertyMetadata(0d));
    }
}
