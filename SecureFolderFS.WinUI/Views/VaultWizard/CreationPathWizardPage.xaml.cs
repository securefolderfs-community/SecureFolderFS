using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard;
using System;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using SecureFolderFS.Sdk.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.Views.VaultWizard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreationPathWizardPage : Page, IDisposable
    {
        private IFileSystemService FileSystemService { get; } = Ioc.Default.GetRequiredService<IFileSystemService>();

        public VaultWizardCreationPathViewModel ViewModel
        {
            get => (VaultWizardCreationPathViewModel)DataContext;
            set => DataContext = value;
        }

        public CreationPathWizardPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VaultWizardCreationPathViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }

        public void Dispose()
        {
            ViewModel.Dispose();
        }

        private async void TextInput_LostFocus(object sender, RoutedEventArgs e)
        {
            var folder = await FileSystemService.GetFolderFromPathAsync(TextInput.Text);
            if (folder is not null)
                await ViewModel.SetLocation(folder);
        }

        private void TextInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.PrimaryButtonEnabled = false;
        }
    }
}
