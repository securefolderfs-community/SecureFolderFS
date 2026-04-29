using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.SecureStore;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Authentication
{
    [Bindable(true)]
    public sealed class PersistedAuthenticationViewModel : AuthenticationViewModel
    {
        private readonly string _vaultId;

        /// <inheritdoc/>
        public override bool CanComplement { get; } = false;

        /// <inheritdoc/>
        public override AuthenticationStage Availability { get; } = AuthenticationStage.Any;

        /// <inheritdoc/>
        public override event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;

        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        public PersistedAuthenticationViewModel(string vaultId)
            : base(string.Empty)
        {
            _vaultId = vaultId;
        }

        /// <inheritdoc/>
        public override Task RevokeAsync(string? id, CancellationToken cancellationToken = default)
        {
            return Task.FromException(new NotSupportedException("Revocation is not supported for persisted authentication."));
        }

        /// <inheritdoc/>
        public override Task<IResult<IKeyBytes>> EnrollAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            return Task.FromException<IResult<IKeyBytes>>(new NotSupportedException("Enrollment is not supported for persisted authentication."));
        }

        /// <inheritdoc/>
        public override Task<IResult<IKeyBytes>> AcquireAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            return Task.FromException<IResult<IKeyBytes>>(new NotSupportedException("Acquisition is not supported for persisted authentication."));
        }

        /// <inheritdoc/>
        protected override async Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
            if (!PersistedCredentialsModel.Instance.Credentials.TryGetValue(_vaultId, out var credentials))
            {
                PersistedCredentialsModel.Instance.Credentials.Remove(_vaultId);
                credentials = ManagedKey.Empty;
            }

            var tcs = new TaskCompletionSource();
            CredentialsProvided?.Invoke(this, new(credentials.CreateCopy(), tcs));
            await tcs.Task;
        }
    }
}