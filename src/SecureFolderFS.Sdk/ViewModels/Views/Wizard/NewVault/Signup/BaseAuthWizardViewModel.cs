using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Shared.Utilities;
using System;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard.NewVault.Signup
{
    public abstract partial class BaseAuthWizardViewModel : ObservableObject, INotifyStateChanged, IDisposable
    {
        [ObservableProperty] private string _DisplayName;
        [ObservableProperty] private string _Description;

        /// <inheritdoc/>
        public abstract event EventHandler<EventArgs>? StateChanged;

        public AuthenticationModel AuthenticationModel { get; }

        protected BaseAuthWizardViewModel(AuthenticationModel authenticationModel)
        {
            AuthenticationModel = authenticationModel;
            _DisplayName = authenticationModel.Name;
            _Description = $"Create authentication for this vault using {authenticationModel.Name}";
        }

        public abstract IDisposable? GetAuthentication();

        /// <inheritdoc/>
        public abstract void Dispose();
    }
}
