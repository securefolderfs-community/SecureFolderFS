using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using SecureFolderFS.AvaloniaUI.Events;
using System.Linq;

namespace SecureFolderFS.AvaloniaUI.UserControls
{
    internal partial class Page : UserControl
    {
        protected ContentPresenter ContentPresenter => (ContentPresenter)this.GetVisualChildren().First();

        public Page()
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