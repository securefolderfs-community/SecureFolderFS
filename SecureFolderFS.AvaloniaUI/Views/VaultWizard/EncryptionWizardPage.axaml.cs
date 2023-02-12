using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.AvaloniaUI.Events;
using SecureFolderFS.AvaloniaUI.UserControls;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard;
using SecureFolderFS.Sdk.ViewModels.Pages.VaultWizard.NewVault;
using System.Collections.ObjectModel;

namespace SecureFolderFS.AvaloniaUI.Views.VaultWizard
{
    internal sealed partial class EncryptionWizardPage : Page
    {
        private IVaultService VaultService { get; } = Ioc.Default.GetRequiredService<IVaultService>();

        public static readonly StyledProperty<VaultWizardEncryptionViewModel> ViewModelProperty
            = AvaloniaProperty.Register<EncryptionWizardPage, VaultWizardEncryptionViewModel>(nameof(ViewModel));

        public static readonly StyledProperty<ObservableCollection<CipherItemViewModel>> ContentCiphersProperty
            = AvaloniaProperty.Register<EncryptionWizardPage, ObservableCollection<CipherItemViewModel>>(nameof(ContentCiphers));

        public static readonly StyledProperty<ObservableCollection<CipherItemViewModel>> FileNameCiphersProperty
            = AvaloniaProperty.Register<EncryptionWizardPage, ObservableCollection<CipherItemViewModel>>(nameof(FileNameCiphers));

        public ObservableCollection<CipherItemViewModel> ContentCiphers
        {
            get => GetValue(ContentCiphersProperty);
            set => SetValue(ContentCiphersProperty, value);
        }

        public ObservableCollection<CipherItemViewModel> FileNameCiphers
        {
            get => GetValue(FileNameCiphersProperty);
            set => SetValue(FileNameCiphersProperty, value);
        }

        public VaultWizardEncryptionViewModel ViewModel
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

        // TODO Replace this workaround with something better
        private void ComboBox_OnLoaded(object? sender, RoutedEventArgs e)
        {
            if (sender is ComboBox comboBox)
                comboBox.SelectedIndex = 0;
        }
    }
}