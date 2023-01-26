using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard;
using SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard.NewVault;
using System.Collections.ObjectModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.Views.VaultWizard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EncryptionWizardPage : Page
    {
        private IVaultService VaultService { get; } = Ioc.Default.GetRequiredService<IVaultService>();

        public ObservableCollection<CipherItemViewModel> ContentCiphers { get; }

        public ObservableCollection<CipherItemViewModel> FileNameCiphers { get; }

        public VaultWizardEncryptionViewModel ViewModel
        {
            get => (VaultWizardEncryptionViewModel)DataContext;
            set => DataContext = value;
        }

        public EncryptionWizardPage()
        {
            InitializeComponent();

            ContentCiphers = new();
            FileNameCiphers = new();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VaultWizardEncryptionViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }

        private void EncryptionWizardPage_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var item in VaultService.GetContentCiphers())
                ContentCiphers.Add(new(item));

            foreach (var item in VaultService.GetFileNameCiphers())
                FileNameCiphers.Add(new(item));
        }
    }
}
