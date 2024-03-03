using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    public abstract partial class AuthenticationViewModel(string id, IFolder vaultFolder)
        : ReportableViewModel, IAuthenticator, IDisposable
    {
        [ObservableProperty] private string? _DisplayName;

        public string Id { get; } = id;

        public IFolder VaultFolder { get; } = vaultFolder;

        /// <summary>
        /// Occurs when credentials have been provided by the user.
        /// </summary>
        public abstract event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;

        /// <inheritdoc/>
        public override void SetError(IResult? result)
        {
            _ = result;
        }

        /// <inheritdoc/>
        public abstract Task RevokeAsync(string id, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract Task<IKey> CreateAsync(string id, byte[]? data, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract Task<IKey> SignAsync(string id, byte[]? data, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public virtual void Dispose()
        {
        }
    }
}
