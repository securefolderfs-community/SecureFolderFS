using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.AppModels;
using System;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard.NewVault.Signup
{
    public abstract partial class BaseAuthWizardViewModel : ObservableObject, IDisposable
    {
        public AuthenticationModel AuthenticationModel { get; }

        [ObservableProperty] private string _DisplayName;
        [ObservableProperty] private string _Description;

        protected BaseAuthWizardViewModel(AuthenticationModel authenticationModel)
        {
            AuthenticationModel = authenticationModel;
            _DisplayName = authenticationModel.AuthenticationName;
            _Description = $"Create authentication for this vault using {authenticationModel.AuthenticationName}";
        }

        public abstract IDisposable? GetAuthentication();

        /// <inheritdoc/>
        public abstract void Dispose();
    }
}
