using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using System;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard.NewVault.Signup
{
    public sealed partial class PasswordWizardViewModel : BaseAuthWizardViewModel
    {
        private readonly DialogViewModel _dialogViewModel;
        [ObservableProperty] private string? _FirstPassword;
        [ObservableProperty] private string? _SecondPassword;

        public PasswordWizardViewModel(DialogViewModel dialogViewModel, AuthenticationModel authenticationModel)
            : base(authenticationModel)
        {
            _dialogViewModel = dialogViewModel;
        }

        public override IDisposable? GetAuthentication()
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        partial void OnFirstPasswordChanged(string? value)
        {
            _dialogViewModel.PrimaryButtonEnabled = !string.IsNullOrWhiteSpace(FirstPassword) && FirstPassword == SecondPassword;
        }

        partial void OnSecondPasswordChanged(string? value)
        {
            _dialogViewModel.PrimaryButtonEnabled = !string.IsNullOrWhiteSpace(FirstPassword) && FirstPassword == SecondPassword;
        }
    }
}
