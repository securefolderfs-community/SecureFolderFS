using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.Dokany.AppModels;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.FUSE.AppModels;
using SecureFolderFS.Core.WebDav.AppModels;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using static SecureFolderFS.Core.Constants;

namespace SecureFolderFS.UI.Helpers
{
    public static class VaultHelpers
    {
        public static async Task<string> GetBestFileSystemAsync(CancellationToken cancellationToken)
        {
            var vaultService = Ioc.Default.GetRequiredService<IVaultService>();
            var settingsService = Ioc.Default.GetRequiredService<ISettingsService>();

            string? lastBestId = null;
            foreach (var item in vaultService.GetFileSystems())
            {
                if (item.Id == settingsService.UserSettings.PreferredFileSystemId)
                {
                    if ((await item.GetStatusAsync(cancellationToken)).Successful)
                        return item.Id;
                }
                else
                {
                    if (lastBestId is null && (await item.GetStatusAsync(cancellationToken)).Successful)
                        lastBestId = item.Id;
                }
            }

            if (lastBestId is null)
                throw new NotSupportedException("No supported adapters found.");

            return lastBestId;
        }

        public static MountOptions GetMountOptions(string fileSystemId)
        {
            return fileSystemId switch
            {
                Core.Constants.FileSystemId.FS_DOKAN => new DokanyMountOptions(),
                Core.Constants.FileSystemId.FS_FUSE => new FuseMountOptions(),
                Core.Constants.FileSystemId.FS_WEBDAV => new WebDavMountOptions() { Domain = "localhost", PreferredPort = 4949 },
                _ => throw new ArgumentOutOfRangeException(nameof(fileSystemId))
            };
        }

        public static IDictionary<string, string?> ParseOptions(VaultOptions vaultOptions)
        {
            return new Dictionary<string, string?>()
            {
                { Associations.ASSOC_CONTENT_CIPHER_ID, vaultOptions.ContentCipherId },
                { Associations.ASSOC_FILENAME_CIPHER_ID, vaultOptions.FileNameCipherId },
                { Associations.ASSOC_AUTHENTICATION, vaultOptions.AuthenticationMethod },
                { Associations.ASSOC_VAULT_ID, vaultOptions.VaultId }
            };
        }

        public static SecretKey ParsePasskeySecret(IKey passkey)
        {
            var keyArray = passkey.ToArray();
            var secretKey = new SecureKey(keyArray.Length);

            keyArray.CopyTo(secretKey.Key, 0);
            Array.Clear(keyArray);

            return secretKey;
        }
    }
}
