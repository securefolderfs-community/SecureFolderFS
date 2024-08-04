using System.Threading.Tasks;
using CommunityToolkit.WinUI.UI.Animations;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls
{

    public sealed partial class BackButtonTitleControl : UserControl
    {
        public event RoutedEventHandler Click;

        public BackButtonTitleControl()
        {
            InitializeComponent();
        }

        public async Task ShowBackAsync()
        {
            GoBack.Visibility = Visibility.Visible;
            await ShowBackButtonStoryboard.BeginAsync();
            ShowBackButtonStoryboard.Stop();
        }

        public async Task HideBackAsync()
        {
            await HideBackButtonStoryboard.BeginAsync();
            HideBackButtonStoryboard.Stop();
            GoBack.Visibility = Visibility.Collapsed;
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(this, e);
        }

        public string? Title
        {
            get => (string?)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(BackButtonTitleControl), new PropertyMetadata(null));
    }
}
