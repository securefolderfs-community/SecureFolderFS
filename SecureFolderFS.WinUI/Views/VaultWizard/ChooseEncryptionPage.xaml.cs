using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.Views.VaultWizard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChooseEncryptionPage : Page, IDisposable
    {
        public ChooseEncryptionPageViewModel ViewModel
        {
            get => (ChooseEncryptionPageViewModel)DataContext;
            set => DataContext = value;
        }

        public ChooseEncryptionPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is ChooseEncryptionPageViewModel viewModel)
            {
                ViewModel = viewModel;
            }

            base.OnNavigatedTo(e);
        }

        public void Dispose()
        {
            ViewModel.Dispose();
        }
    }
}
