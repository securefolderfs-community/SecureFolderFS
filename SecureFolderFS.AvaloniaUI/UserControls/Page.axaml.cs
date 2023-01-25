using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Navigation;

namespace SecureFolderFS.AvaloniaUI.UserControls
{
    internal partial class Page : UserControl
    {
        public Page()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public virtual void OnNavigatedTo(NavigationEventArgs e)
        {
        }
    }
}