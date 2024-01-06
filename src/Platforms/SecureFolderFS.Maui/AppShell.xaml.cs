using SecureFolderFS.Maui.Views.Vault;

namespace SecureFolderFS.Maui
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes
            Routing.RegisterRoute("LoginPage", typeof(LoginPage));
        }
    }
}
