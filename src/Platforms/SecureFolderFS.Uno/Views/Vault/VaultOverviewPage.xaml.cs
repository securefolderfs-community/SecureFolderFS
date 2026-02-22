using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.Utils;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Views.Vault
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [INotifyPropertyChanged]
    public sealed partial class VaultOverviewPage : Page, IEmbeddedControlContent
    {
        public VaultOverviewViewModel? ViewModel
        {
            get => DataContext.TryCast<VaultOverviewViewModel>();
            set { DataContext = value; OnPropertyChanged(); }
        }
        
        /// <inheritdoc/>
        public object? EmbeddedContent { get => field ??= CreateEmbeddedContent(); }

        public VaultOverviewPage()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VaultOverviewViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }

        private DependencyObject? CreateEmbeddedContent()
        {
            if (Resources.TryGetValue("WidgetReorderButtonTemplate", out var resource) && resource is DataTemplate template)
            {
                var content = template.LoadContent();
                if (content is FrameworkElement element)
                    element.DataContext = ViewModel;

                return content;
            }

            return null;
        }
    }
}
