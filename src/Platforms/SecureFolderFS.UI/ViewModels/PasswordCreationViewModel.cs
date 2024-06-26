using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.EventArguments;

namespace SecureFolderFS.UI.ViewModels
{
    /// <inheritdoc cref="PasswordViewModel"/>
    [Bindable(true)]
    public sealed partial class PasswordCreationViewModel : PasswordViewModel
    {
        [ObservableProperty] private string? _SecondaryPassword;

        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        /// <inheritdoc/>
        public override event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;

        public PasswordCreationViewModel(string id)
            : base(id)
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
        protected override Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
            // TODO: Maybe opt-in to something similar like in PasswordLoginViewModel, where CredentialsProvided is also used?
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            SecondaryPassword = null;
            base.Dispose();
        }
    }
}
