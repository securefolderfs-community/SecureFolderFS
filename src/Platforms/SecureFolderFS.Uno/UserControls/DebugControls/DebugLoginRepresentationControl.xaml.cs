using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.ViewModels.Views.Host;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Uno.UserControls.InterfaceRoot;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SecureFolderFS.Uno.UserControls.DebugControls
{
    public sealed partial class DebugLoginRepresentationControl : UserControl
    {
        private MainWindowRootControl? _rootControl;

        public DebugLoginRepresentationControl()
        {
            InitializeComponent();

            _rootControl = App.Instance?.MainWindow?.Content as MainWindowRootControl;
        }

        private void RestartLoginView_Click(object sender, RoutedEventArgs e)
        {
            if (_rootControl is null)
                return;

            if (_rootControl.RootNavigationService.CurrentView is not MainHostViewModel mainHost)
                return;

            if (mainHost.NavigationService.CurrentView is not VaultLoginViewModel vaultLogin)
                return;

            _ = vaultLogin.InitAsync();
        }
    }
}
