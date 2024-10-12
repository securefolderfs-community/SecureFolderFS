using System.Windows.Input;

namespace SecureFolderFS.Maui.UserControls
{
    public partial class RecoveryPreviewControl : ContentView
    {
        public RecoveryPreviewControl()
        {
            InitializeComponent();
            RootGrid.BindingContext = this;
        }
        
        public string? RecoveryKey
        {
            get => (string?)GetValue(RecoveryKeyProperty);
            set => SetValue(RecoveryKeyProperty, value);
        }
        public static readonly BindableProperty RecoveryKeyProperty =
            BindableProperty.Create(nameof(RecoveryKey), typeof(string), typeof(RecoveryPreviewControl), null);
        
        public ICommand? ExportCommand
        {
            get => (ICommand?)GetValue(ExportCommandProperty);
            set => SetValue(ExportCommandProperty, value);
        }
        public static readonly BindableProperty ExportCommandProperty =
            BindableProperty.Create(nameof(ExportCommand), typeof(ICommand), typeof(RecoveryPreviewControl), null);
    }
}

