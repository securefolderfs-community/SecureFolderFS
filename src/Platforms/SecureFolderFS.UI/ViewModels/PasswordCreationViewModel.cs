using System;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Storage;

namespace SecureFolderFS.UI.ViewModels
{
    public sealed partial class PasswordCreationViewModel : PasswordViewModel
    {
        [ObservableProperty] private string? _SecondaryPassword;

        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        /// <inheritdoc/>
        public override event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;

        public PasswordCreationViewModel(string id, IFolder vaultFolder)
            : base(id, vaultFolder)
        {
        }

        partial void OnSecondaryPasswordChanged(string? value)
        {
            StateChanged?.Invoke(this, new PasswordChangedEventArgs(PrimaryPassword == SecondaryPassword));
        }

        /// <inheritdoc/>
        protected override void OnPasswordChanged(string? value)
        {
            if (PrimaryPassword == SecondaryPassword)
                StateChanged?.Invoke(this, new PasswordChangedEventArgs(PrimaryPassword == SecondaryPassword));
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            SecondaryPassword = null;
            base.Dispose();
        }
    }
}
