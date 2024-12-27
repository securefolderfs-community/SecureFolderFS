using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.EventArguments;

namespace SecureFolderFS.UI.ViewModels.Authentication
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

        partial void OnSecondaryPasswordChanged(string? value)
        {
            StateChanged?.Invoke(this, new PasswordChangedEventArgs(PrimaryPassword == SecondaryPassword));
        }

        /// <inheritdoc/>
        protected override void OnPasswordChanged(string? value)
        {
            StateChanged?.Invoke(this, new PasswordChangedEventArgs(PrimaryPassword == SecondaryPassword));
        }

        /// <inheritdoc/>
        public override Task RevokeAsync(string? id, CancellationToken cancellationToken = default)
        {
            SecondaryPassword = null;
            return base.RevokeAsync(id, cancellationToken);
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
            _SecondaryPassword = null;
            base.Dispose();
        }
    }
}
