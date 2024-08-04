using SecureFolderFS.Maui.Views.Vault;
using SecureFolderFS.Sdk.ViewModels;

namespace SecureFolderFS.Maui
{
    public partial class AppShell : Shell
    {
        public MainViewModel MainViewModel { get; } = new();

        public AppShell()
        {
            InitializeComponent();

            // Register routes
            Routing.RegisterRoute("LoginPage", typeof(LoginPage));
            Routing.RegisterRoute("OverviewPage", typeof(OverviewPage));
            //Routing.RegisterRoute("PropertiesPage", typeof(PropertiesPage)); // TODO
        }

        private async void AppShell_Loaded(object? sender, EventArgs e)
        {
            await MainViewModel.InitAsync();
        }
    }
}
