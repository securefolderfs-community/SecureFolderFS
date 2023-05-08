using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SecureFolderFS.AvaloniaUI.Events;
using SecureFolderFS.AvaloniaUI.UserControls;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard.NewVault;

namespace SecureFolderFS.AvaloniaUI.Views.VaultWizard
{
    internal sealed partial class EncryptionWizardPage : Page
    {
        public static readonly StyledProperty<EncryptionWizardViewModel> ViewModelProperty
            = AvaloniaProperty.Register<EncryptionWizardPage, EncryptionWizardViewModel>(nameof(ViewModel));

        public EncryptionWizardViewModel ViewModel
        {
            get => GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public EncryptionWizardPage()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is EncryptionWizardViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }

        // TODO Replace this workaround with something better
        private void ComboBox_Loaded(object? sender, RoutedEventArgs e)
        {
            if (sender is ComboBox comboBox)
                comboBox.SelectedIndex = 0;
        }
    }
}