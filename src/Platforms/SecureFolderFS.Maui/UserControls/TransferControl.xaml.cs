using System.Windows.Input;

namespace SecureFolderFS.Maui.UserControls
{
    public partial class TransferControl : ContentView
    {
        public TransferControl()
        {
            InitializeComponent();
        }
        
        public bool CanCancel
        {
            get => (bool)GetValue(CanCancelProperty);
            set => SetValue(CanCancelProperty, value);
        }
        public static readonly BindableProperty CanCancelProperty =
            BindableProperty.Create(nameof(CanCancel), typeof(bool), typeof(TransferControl), defaultValue: true);
        
        public string? Title
        {
            get => (string?)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(TransferControl), defaultValue: null);
        
        public string? PrimaryButtonText
        {
            get => (string?)GetValue(PrimaryButtonTextProperty);
            set => SetValue(PrimaryButtonTextProperty, value);
        }
        public static readonly BindableProperty PrimaryButtonTextProperty =
            BindableProperty.Create(nameof(PrimaryButtonText), typeof(string), typeof(TransferControl), defaultValue: null);
        
        public ICommand? CancelCommand
        {
            get => (ICommand?)GetValue(CancelCommandProperty);
            set => SetValue(CancelCommandProperty, value);
        }
        public static readonly BindableProperty CancelCommandProperty =
            BindableProperty.Create(nameof(CancelCommand), typeof(ICommand), typeof(TransferControl), defaultValue: null);
        
        public ICommand? PrimaryCommand
        {
            get => (ICommand?)GetValue(PrimaryCommandProperty);
            set => SetValue(PrimaryCommandProperty, value);
        }
        public static readonly BindableProperty PrimaryCommandProperty =
            BindableProperty.Create(nameof(PrimaryCommand), typeof(ICommand), typeof(TransferControl), defaultValue: null);
    }
}
