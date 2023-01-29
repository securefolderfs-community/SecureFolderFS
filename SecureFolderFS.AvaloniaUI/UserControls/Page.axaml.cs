using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using SecureFolderFS.AvaloniaUI.Events;

namespace SecureFolderFS.AvaloniaUI.UserControls
{
    internal partial class Page : UserControl
    {
        protected ContentPresenter ContentPresenter => (ContentPresenter)this.GetVisualChildren().First();

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