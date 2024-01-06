using SecureFolderFS.Maui.Views.Vault;
using SecureFolderFS.Sdk.ViewModels;

namespace SecureFolderFS.Maui
{
    public partial class AppShell : Shell
    {
        public MainViewModel MainViewModel { get; } = new MainViewModel();

        public AppShell()
        {
            InitializeComponent();

            // Register routes
            Routing.RegisterRoute("LoginPage", typeof(LoginPage));
        }

        private void AppShell_Loaded(object? sender, EventArgs e)
        {
            _ = MainViewModel.InitAsync();
        }
    }
}
