using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Extensions;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SecureFolderFS.Uno.UserControls
{
    public sealed partial class ErrorControl : UserControl
    {
        public ErrorControl()
        {
            InitializeComponent();
        }

        public string? ExceptionMessage
        {
            get => (string?)GetValue(ExceptionMessageProperty);
            set => SetValue(ExceptionMessageProperty, value);
        }
        public static readonly DependencyProperty ExceptionMessageProperty =
            DependencyProperty.Register(nameof(ExceptionMessage), typeof(string), typeof(ErrorControl), new PropertyMetadata(null));

        public string? Title
        {
            get => (string?)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(ErrorControl), new PropertyMetadata("ErrorOccurred".ToLocalized()));
    }
}
