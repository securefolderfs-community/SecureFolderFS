using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.UI.ViewModels
{
    /// <inheritdoc cref="AuthenticationViewModel"/>
    [Bindable(true)]
    public abstract partial class PasswordViewModel : AuthenticationViewModel, IWrapper<IKey>
    {
        [ObservableProperty] private string? _PrimaryPassword;

        /// <inheritdoc/>
        public virtual IKey Inner => TryGetPasswordAsKey() ?? new DisposablePassword(string.Empty);

        /// <inheritdoc/>
        public sealed override AuthenticationType Availability { get; } = AuthenticationType.FirstStageOnly;

        protected PasswordViewModel(string id)
            : base(id)
        {
            DisplayName = "Password".ToLocalized();
            Icon = "\uE8AC";
        }

        partial void OnPrimaryPasswordChanged(string? value)
        {
            OnPasswordChanged(value);
        }

        /// <inheritdoc/>
        public override Task RevokeAsync(string id, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override Task<IKey> CreateAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(TryGetPasswordAsKey() ?? throw new InvalidOperationException("The password is not ready yet."));
        }

        /// <inheritdoc/>
        public override Task<IKey> SignAsync(string id, byte[] data, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(TryGetPasswordAsKey() ?? throw new InvalidOperationException("The password is not ready yet."));
        }

        /// <summary>
        /// Notifies that the <see cref="PrimaryPassword"/> property has changed.
        /// </summary>
        /// <param name="value">The new value.</param>
        protected virtual void OnPasswordChanged(string? value)
        {
            _ = value;
        }

        /// <summary>
        /// Tries to retrieve <see cref="PrimaryPassword"/> as a <see cref="IKey"/> instance.
        /// </summary>
        /// <returns>A new instance of <see cref="IKey"/> that represents the password.</returns>
        protected virtual IKey? TryGetPasswordAsKey()
        {
            return !string.IsNullOrEmpty(PrimaryPassword) ? new DisposablePassword(PrimaryPassword) : null;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            PrimaryPassword = null;
        }
    }
}
