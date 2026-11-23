using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Journal;

namespace SecureFolderFS.Uno.UserControls.Widgets
{
    public sealed partial class JournalWidget : UserControl
    {
        public JournalWidget()
        {
            InitializeComponent();
        }

        public JournalWidgetViewModel? ViewModel
        {
            get => (JournalWidgetViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(JournalWidgetViewModel), typeof(JournalWidget), new PropertyMetadata(null));
    }
}
