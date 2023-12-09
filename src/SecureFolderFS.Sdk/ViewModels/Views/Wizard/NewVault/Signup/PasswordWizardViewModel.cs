using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Shared.Helpers;
using System;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard.NewVault.Signup
{
    public sealed partial class PasswordWizardViewModel : BaseAuthWizardViewModel
    {
        private readonly DialogViewModel _dialogViewModel;

        [ObservableProperty] private string? _FirstPassword;
        [ObservableProperty] private string? _SecondPassword;

        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        public PasswordWizardViewModel(DialogViewModel dialogViewModel, AuthenticationModel authenticationModel)
            : base(authenticationModel)
        {
            _dialogViewModel = dialogViewModel;
        }

        partial void OnFirstPasswordChanged(string? value)
        {
            _dialogViewModel.PrimaryButtonEnabled = !string.IsNullOrWhiteSpace(FirstPassword) && FirstPassword == SecondPassword;
        }

        partial void OnSecondPasswordChanged(string? value)
        {
            _dialogViewModel.PrimaryButtonEnabled = !string.IsNullOrWhiteSpace(FirstPassword) && FirstPassword == SecondPassword;
        }

        /// <inheritdoc/>
        public override IDisposable? GetAuthentication()
        {
            return new DisposablePassword(FirstPassword ?? string.Empty);
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            FirstPassword = null;
            SecondPassword = null;
        }
    }
}
