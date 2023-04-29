using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.AvaloniaUI.Events;
using SecureFolderFS.AvaloniaUI.UserControls;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard.NewVault;
using System.Collections.ObjectModel;

namespace SecureFolderFS.AvaloniaUI.Views.VaultWizard
{
    internal sealed partial class EncryptionWizardPage : Page
    {
        private IVaultService VaultService { get; } = Ioc.Default.GetRequiredService<IVaultService>();

        public static readonly StyledProperty<EncryptionWizardViewModel> ViewModelProperty
            = AvaloniaProperty.Register<EncryptionWizardPage, EncryptionWizardViewModel>(nameof(ViewModel));

        public static readonly StyledProperty<ObservableCollection<CipherInfoViewModel>> ContentCiphersProperty
            = AvaloniaProperty.Register<EncryptionWizardPage, ObservableCollection<CipherInfoViewModel>>(nameof(ContentCiphers));

        public static readonly StyledProperty<ObservableCollection<CipherInfoViewModel>> FileNameCiphersProperty
            = AvaloniaProperty.Register<EncryptionWizardPage, ObservableCollection<CipherInfoViewModel>>(nameof(FileNameCiphers));

        public ObservableCollection<CipherInfoViewModel> ContentCiphers
        {
            get => GetValue(ContentCiphersProperty);
            set => SetValue(ContentCiphersProperty, value);
        }

        public ObservableCollection<CipherInfoViewModel> FileNameCiphers
        {
            get => GetValue(FileNameCiphersProperty);
            set => SetValue(FileNameCiphersProperty, value);
        }

        public EncryptionWizardViewModel ViewModel
        {
            get => GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public EncryptionWizardPage()
        {
            AvaloniaXamlLoader.Load(this);
            ContentCiphers = new();
            FileNameCiphers = new();
        }

        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is EncryptionWizardViewModel viewModel)
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

        // TODO Replace this workaround with something better
        private void ComboBox_OnLoaded(object? sender, RoutedEventArgs e)
        {
            if (sender is ComboBox comboBox)
                comboBox.SelectedIndex = 0;
        }
    }
}