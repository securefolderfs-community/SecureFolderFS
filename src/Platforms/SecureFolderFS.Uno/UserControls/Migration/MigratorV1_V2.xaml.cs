using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls.Migration
{
    public sealed partial class MigratorV1_V2 : UserControl
    {
        private IDisposable? _unlockContract;
        private IVaultMigratorModel? _vaultMigrator;

        private IVaultManagerService VaultManagerService { get; } = DI.Service<IVaultManagerService>();

        public MigratorV1_V2()
        {
            InitializeComponent();
        }

        private async void Migrator_Loaded(object sender, RoutedEventArgs e)
        {
            if (VaultFolder is null)
                return;

            _vaultMigrator = await VaultManagerService.GetMigratorAsync(VaultFolder);
        }

        private async void Password_PasswordSubmitted(object sender, RoutedEventArgs e)
        {
            if (_vaultMigrator is null)
                return;

            if (Password.GetPassword() is not { } password)
                return;

            try
            {
                _unlockContract = await _vaultMigrator.UnlockAsync(password.ToString());

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
            if (_vaultMigrator is null)
                return;

            if (_unlockContract is null)
                return;

            try
            {
                await _vaultMigrator.MigrateAsync(_unlockContract, new());
            }
            catch (Exception ex)
            {
                // TODO: Notify about an error
                _ = ex;
            }
        }

        public string? VaultName
        {
            get => (string?)GetValue(VaultNameProperty);
            set => SetValue(VaultNameProperty, value);
        }
        public static readonly DependencyProperty VaultNameProperty =
            DependencyProperty.Register(nameof(VaultName), typeof(string), typeof(MigratorV1_V2), new PropertyMetadata(null));

        public IFolder? VaultFolder
        {
            get => (IFolder?)GetValue(VaultFolderProperty);
            set => SetValue(VaultFolderProperty, value);
        }
        public static readonly DependencyProperty VaultFolderProperty =
            DependencyProperty.Register(nameof(VaultFolder), typeof(IFolder), typeof(MigratorV1_V2), new PropertyMetadata(null));
    }
}
