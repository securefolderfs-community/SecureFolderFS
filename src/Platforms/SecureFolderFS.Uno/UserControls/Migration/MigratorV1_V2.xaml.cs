using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls.Migration
{
    public sealed partial class MigratorV1_V2 : UserControl
    {
        private IDisposable? _unlockContract;

        public MigratorV1_V2()
        {
            InitializeComponent();
        }

        private async void Password_PasswordSubmitted(object sender, RoutedEventArgs e)
        {
            if (VaultMigrator is null)
                return;

            if (Password.GetPassword() is not { } password)
                return;

            try
            {
                _unlockContract = await VaultMigrator.UnlockAsync(password);

                LoginView.Visibility = Visibility.Collapsed;
                MigrateView.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                // TODO: Notify about an error
                _ = ex;
            }
        }

        private async void Migrate_Click(object sender, RoutedEventArgs e)
        {
            if (VaultMigrator is null)
                return;

            if (_unlockContract is null)
                return;

            var result = await VaultMigrator.MigrateAsync(_unlockContract, new());
            _ = result;
            // TODO: Notify whether the operation was successful or not
        }

        public string? VaultName
        {
            get => (string?)GetValue(VaultNameProperty);
            set => SetValue(VaultNameProperty, value);
        }
        public static readonly DependencyProperty VaultNameProperty =
            DependencyProperty.Register(nameof(VaultName), typeof(string), typeof(MigratorV1_V2), new PropertyMetadata(null));

        public IVaultMigratorModel? VaultMigrator
        {
            get => (IVaultMigratorModel?)GetValue(VaultMigratorProperty);
            set => SetValue(VaultMigratorProperty, value);
        }
        public static readonly DependencyProperty VaultMigratorProperty =
            DependencyProperty.Register(nameof(VaultMigrator), typeof(IVaultMigratorModel), typeof(MigratorV1_V2), new PropertyMetadata(null));
    }
}
