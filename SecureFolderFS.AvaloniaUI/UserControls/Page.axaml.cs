using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SecureFolderFS.AvaloniaUI.Events;

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

        public virtual void OnNavigatingFrom()
        {
        }
    }
}