using SecureFolderFS.Core.Dokany;
using SecureFolderFS.Core.WebDav;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Utils;
using SecureFolderFS.WinUI.AppModels;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultService"/>
    internal sealed class VaultService : IVaultService
    {
        /// <inheritdoc/>
        public bool IsFileNameReserved(string? fileName)
        {
            return fileName is not null &&
                   (fileName.Equals(Core.Constants.VAULT_KEYSTORE_FILENAME, StringComparison.Ordinal) ||
                    fileName.Equals(Core.Constants.VAULT_CONFIGURATION_FILENAME, StringComparison.Ordinal) ||
                    fileName.Equals(Core.Constants.CONTENT_FOLDERNAME, StringComparison.Ordinal));
        }

        /// <inheritdoc/>
        public IAsyncValidator<IFolder> GetVaultValidator()
        {
            return VaultHelpers.NewVaultValidator(StreamSerializer.Instance);
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IFileSystemInfoModel> GetFileSystemsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return new DokanyFileSystemDescriptor();
            yield return new WebDavFileSystemDescriptor();

            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<CipherInfoModel> GetContentCiphersAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return new CipherInfoModel("XChaCha20-Poly1305", Core.Constants.CipherId.XCHACHA20_POLY1305);
            yield return new CipherInfoModel("AES-GCM", Core.Constants.CipherId.AES_GCM);
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<CipherInfoModel> GetFileNameCiphersAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return new CipherInfoModel("AES-SIV", Core.Constants.CipherId.AES_SIV);
            yield return new CipherInfoModel("None", Core.Constants.CipherId.NONE);
            await Task.CompletedTask;
        }
    }
}
