using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    public abstract partial class AuthenticationViewModel : ReportableViewModel, IAuthenticator, IDisposable
    {
        [ObservableProperty] private string? _DisplayName;

        public string Id { get; }

        public IFolder VaultFolder { get; }

        protected AuthenticationViewModel(string id, IFolder vaultFolder)
        {
            Id = id;
            VaultFolder = vaultFolder;
        }

        /// <inheritdoc/>
        public override void SetError(IResult? result)
        {
            _ = result;
        }

        /// <summary>
        /// Retrieves the authentication key, if available.
        /// </summary>
        /// <returns>If the authentication was performed, returns a new <see cref="IKey"/> instance; otherwise null.</returns>
        public abstract IKey? RetrieveKey();

        /// <inheritdoc/>
        public abstract Task RevokeAsync(string id, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract Task<IKey> CreateAsync(string id, byte[]? data, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract Task<IKey> SignAsync(string id, byte[] data, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract void Dispose();
    }
}
