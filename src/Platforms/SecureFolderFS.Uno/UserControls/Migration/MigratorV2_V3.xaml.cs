using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OwlCore.Storage;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.Utils;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls.Migration
{
    [ObservableObject]
    public sealed partial class MigratorV2_V3 : UserControl, IMigratorControl
    {
        private readonly KeySequence _keySequence = new();
        private Iterator<AuthenticationViewModel>? _loginSequence;

        [ObservableProperty] private ReportableViewModel? _CurrentViewModel;

        public MigratorV2_V3()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public async Task ContinueAsync()
        {
            if (CurrentViewModel is AuthenticationViewModel authenticationViewModel)
                await authenticationViewModel.ProvideCredentialsCommand.ExecuteAsync(null);
        }

        /// <inheritdoc/>
        public void Report(IResult? value)
        {
            if (value is null)
                return;

            if (!value.Successful)
                RestartLoginProcess();
        }

        private async Task BeginAuthenticationAsync(IFolder vaultFolder)
        {
            var vaultCredentialsService = DI.Service<IVaultCredentialsService>();
            _loginSequence = new(await vaultCredentialsService.GetLoginAsync(vaultFolder).ToArrayAsync());

            // Set up the first authentication method
            var result = ProceedAuthentication();
            if (!result.Successful)
                CurrentViewModel = new ErrorViewModel(result);
        }

        private IResult ProceedAuthentication()
        {
            try
            {
                if (_loginSequence is null || !_loginSequence.MoveNext())
                    return new MessageResult(false, "No authentication methods available.");

                // Get the appropriate method
                var viewModel = _loginSequence.Current;
                CurrentViewModel = viewModel;

                return Result.Success;
            }
            catch (Exception ex)
            {
                return Result.Failure(ex);
            }
        }

        private void RestartLoginProcess()
        {
            // Dispose built key sequence
            _keySequence.Dispose();

            // Reset login sequence only if chain is longer than one authentication
            if (_loginSequence?.Count > 1)
            {
                _loginSequence?.Reset();
                var result = ProceedAuthentication();
                if (!result.Successful)
                    CurrentViewModel = new ErrorViewModel(result);
            }
        }

        private async void CurrentViewModel_CredentialsProvided(object? sender, CredentialsProvidedEventArgs e)
        {
            // Add authentication
            _keySequence.Add(e.Authentication);

            var result = ProceedAuthentication();
            if (!result.Successful && CurrentViewModel is not ErrorViewModel)
            {
                // Reached the end in which case we should try to unlock the vault
                MigrateCommand?.Execute(_keySequence);
            }
        }

        partial void OnCurrentViewModelChanged(ReportableViewModel? oldValue, ReportableViewModel? newValue)
        {
            // Detach old
            if (oldValue is AuthenticationViewModel oldViewModel)
                oldViewModel.CredentialsProvided -= CurrentViewModel_CredentialsProvided;

            // Attach new
            if (newValue is AuthenticationViewModel newViewModel)
                newViewModel.CredentialsProvided += CurrentViewModel_CredentialsProvided;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        public ICommand? MigrateCommand
        {
            get => (ICommand?)GetValue(MigrateCommandProperty);
            set => SetValue(MigrateCommandProperty, value);
        }

        public static readonly DependencyProperty MigrateCommandProperty =
            DependencyProperty.Register(nameof(MigrateCommand), typeof(ICommand), typeof(MigratorV2_V3), new PropertyMetadata(null));

        public string? VaultName
        {
            get => (string?)GetValue(VaultNameProperty);
            set => SetValue(VaultNameProperty, value);
        }
        public static readonly DependencyProperty VaultNameProperty =
            DependencyProperty.Register(nameof(VaultName), typeof(string), typeof(MigratorV2_V3), new PropertyMetadata(null));

        public IFolder? VaultFolder
        {
            get => (IFolder?)GetValue(VaultFolderProperty);
            set => SetValue(VaultFolderProperty, value);
        }
        public static readonly DependencyProperty VaultFolderProperty =
            DependencyProperty.Register(nameof(VaultFolder), typeof(IFolder), typeof(MigratorV2_V3), new PropertyMetadata(null,
                async (s, e) =>
                {
                    if (s is not MigratorV2_V3 migratorV2V3)
                        return;
                    
                    if (e.NewValue is not IFolder vaultFolder)
                        return;

                    await migratorV2V3.BeginAuthenticationAsync(vaultFolder);
                }));

        public bool ProvideContinuationButton
        {
            get => (bool)GetValue(ProvideContinuationButtonProperty);
            set => SetValue(ProvideContinuationButtonProperty, value);
        }
        public static readonly DependencyProperty ProvideContinuationButtonProperty =
            DependencyProperty.Register(nameof(ProvideContinuationButton), typeof(bool), typeof(MigratorV2_V3), new PropertyMetadata(true));
    }
}
