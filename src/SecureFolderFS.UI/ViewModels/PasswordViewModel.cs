using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.UI.ViewModels
{
    /// <inheritdoc cref="AuthenticationViewModel"/>
    public abstract partial class PasswordViewModel : AuthenticationViewModel
    {
        [ObservableProperty] private string? _PrimaryPassword;

        protected PasswordViewModel(string id, IFolder vaultFolder)
            : base(id, vaultFolder)
        {
        }

        partial void OnPrimaryPasswordChanged(string? value)
        {
            OnPasswordChanged(value);
        }

        /// <inheritdoc/>
        public override IKey? RetrieveKey()
        {
            return !string.IsNullOrEmpty(PrimaryPassword) ? new DisposablePassword(PrimaryPassword) : null;
        }

        /// <inheritdoc/>
        public override Task RevokeAsync(string id, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override Task<IKey> CreateAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IKey>(RetrieveKey() ?? throw new InvalidOperationException("The password is not ready yet."));
        }

        /// <inheritdoc/>
        public override Task<IKey> SignAsync(string id, byte[] data, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IKey>(RetrieveKey() ?? throw new InvalidOperationException("The password is not ready yet."));
        }

        /// <summary>
        /// Notifies that the <see cref="PrimaryPassword"/> property has changed.
        /// </summary>
        /// <param name="value">The new value.</param>
        protected virtual void OnPasswordChanged(string? value)
        {
            _ = value;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            PrimaryPassword = null;
        }
    }
}
