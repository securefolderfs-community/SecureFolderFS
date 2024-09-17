using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SecureFolderFS.Maui.UserControls
{
    public partial class RecoveryPreviewControl : ContentView
    {
        public RecoveryPreviewControl()
        {
            BindingContext = this;
            InitializeComponent();
        }
        
        public string? MasterKey
        {
            get => (string?)GetValue(MasterKeyProperty);
            set => SetValue(MasterKeyProperty, value);
        }
        public static readonly BindableProperty MasterKeyProperty =
            BindableProperty.Create(nameof(MasterKey), typeof(string), typeof(RecoveryPreviewControl), null);
        
        public ICommand? ExportCommand
        {
            get => (ICommand?)GetValue(ExportCommandProperty);
            set => SetValue(ExportCommandProperty, value);
        }
        public static readonly BindableProperty ExportCommandProperty =
            BindableProperty.Create(nameof(ExportCommand), typeof(ICommand), typeof(RecoveryPreviewControl), null);
    }
}

