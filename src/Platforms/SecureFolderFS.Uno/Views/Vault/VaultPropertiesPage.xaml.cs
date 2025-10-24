using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared.Extensions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Views.Vault
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [INotifyPropertyChanged]
    public sealed partial class VaultPropertiesPage : Page
    {
        public VaultPropertiesViewModel? ViewModel
        {
            get => DataContext.TryCast<VaultPropertiesViewModel>();
            set { DataContext = value; OnPropertyChanged(); }
        }

        public VaultPropertiesPage()
        {
            InitializeComponent();
            FileSystemText.AddHandler(PointerReleasedEvent, new PointerEventHandler(FileSystemText_PointerReleased), true);
        }

        /// <inheritdoc/>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VaultPropertiesViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }

        private void FileSystemText_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (sender is not TextBlock textBlock)
                return;

            if (!string.IsNullOrEmpty(textBlock.SelectedText))
                return;

            ViewModel?.ToggleFileSystemTextCommand.Execute(null);

            // The TextBlock might sometimes flicker so re-draw is necessary
            textBlock.InvalidateMeasure();
            textBlock.UpdateLayout();
        }
    }
}
