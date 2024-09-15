using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls.Migration
{
    public sealed partial class MigratorV1_V2 : UserControl, IWrapper<object?>
    {
        public MigratorV1_V2()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public object? Inner => Password.GetPassword();

        private void Password_PasswordSubmitted(object sender, RoutedEventArgs e)
        {
            if (Inner is not { } password)
                return;

            MigrateCommand?.Execute(password);
        }

        public ICommand? MigrateCommand
        {
            get => (ICommand?)GetValue(MigrateCommandProperty);
            set => SetValue(MigrateCommandProperty, value);
        }
        public static readonly DependencyProperty MigrateCommandProperty =
            DependencyProperty.Register(nameof(MigrateCommand), typeof(ICommand), typeof(MigratorV1_V2), new PropertyMetadata(null));

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

        public bool ProvideContinuationButton
        {
            get => (bool)GetValue(ProvideContinuationButtonProperty);
            set => SetValue(ProvideContinuationButtonProperty, value);
        }
        public static readonly DependencyProperty ProvideContinuationButtonProperty =
            DependencyProperty.Register(nameof(ProvideContinuationButton), typeof(bool), typeof(MigratorV1_V2), new PropertyMetadata(true));
    }
}
