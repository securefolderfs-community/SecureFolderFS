using System.Windows.Input;
using SecureFolderFS.Sdk.Enums;

namespace SecureFolderFS.Maui.UserControls.Widgets
{
    public partial class HealthWidget : ContentView, IDisposable
    {
        public HealthWidget()
        {
            InitializeComponent();
        }

        private void HealthScanControl_Clicked(object? sender, EventArgs e)
        {
            OpenVaultHealthCommand?.Execute(null);
        }

        public Severity Severity
        {
            get => (Severity)GetValue(SeverityProperty);
            set => SetValue(SeverityProperty, value);
        }
        public static readonly BindableProperty SeverityProperty =
            BindableProperty.Create(nameof(Severity), typeof(Severity), typeof(HealthWidget), Severity.Default);

        public string? StatusTitle
        {
            get => (string?)GetValue(StatusTitleProperty);
            set => SetValue(StatusTitleProperty, value);
        }
        public static readonly BindableProperty StatusTitleProperty =
            BindableProperty.Create(nameof(StatusTitle), typeof(string), typeof(HealthWidget));

        public string? LastCheckedText
        {
            get => (string?)GetValue(LastCheckedTextProperty);
            set => SetValue(LastCheckedTextProperty, value);
        }
        public static readonly BindableProperty LastCheckedTextProperty =
            BindableProperty.Create(nameof(LastCheckedText), typeof(string), typeof(HealthWidget));

        public ICommand? OpenVaultHealthCommand
        {
            get => (ICommand?)GetValue(OpenVaultHealthCommandProperty);
            set => SetValue(OpenVaultHealthCommandProperty, value);
        }
        public static readonly BindableProperty OpenVaultHealthCommandProperty =
            BindableProperty.Create(nameof(OpenVaultHealthCommand), typeof(ICommand), typeof(HealthWidget));

        public bool IsProgressing
        {
            get => (bool)GetValue(IsProgressingProperty);
            set => SetValue(IsProgressingProperty, value);
        }
        public static readonly BindableProperty IsProgressingProperty =
            BindableProperty.Create(nameof(IsProgressing), typeof(bool), typeof(HealthWidget), false);

        /// <inheritdoc/>
        public void Dispose()
        {
            ScanControl.Dispose();
        }
    }
}

