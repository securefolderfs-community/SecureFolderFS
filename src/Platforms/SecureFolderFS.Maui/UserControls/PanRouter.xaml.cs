using System.Windows.Input;

namespace SecureFolderFS.Maui.UserControls
{
    public partial class PanRouter : ContentView, IDisposable
    {
        public PanRouter()
        {
            InitializeComponent();
        }
        
        private void GestureRecognizer_PanUpdated(object? sender, PanUpdatedEventArgs e)
        {
            PanUpdatedCommand?.Execute(e);
        }
        
        /// <inheritdoc/>
        public void Dispose()
        {
            (RouterContent as IDisposable)?.Dispose();
        }

        public object? RouterContent
        {
            get => (object?)GetValue(RouterContentProperty);
            set => SetValue(RouterContentProperty, value);
        }
        public static readonly BindableProperty RouterContentProperty =
            BindableProperty.Create(nameof(RouterContent), typeof(object), typeof(PanRouter));
        
        public ICommand? PanUpdatedCommand
        {
            get => (ICommand?)GetValue(PanUpdatedCommandProperty);
            set => SetValue(PanUpdatedCommandProperty, value);
        }
        public static readonly BindableProperty PanUpdatedCommandProperty =
            BindableProperty.Create(nameof(PanUpdatedCommand), typeof(ICommand), typeof(PanRouter));
    }
}
