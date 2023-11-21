using SecureFolderFS.Cli.Enums;
using SecureFolderFS.Cli.Helpers;
using SecureFolderFS.Cli.Options;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.UI.AppModels;
using SecureFolderFS.UI.ServiceImplementation.Vault;
using SecureFolderFS.UI.Storage.NativeStorage;
using Constants = SecureFolderFS.Core.Cryptography.Constants;

namespace SecureFolderFS.Cli.Handlers
{
    public sealed class CreateVaultHandler : SingletonBase<CreateVaultHandler>, IHandler<CreateVaultOptions>
    {
        public async Task HandleAsync(CreateVaultOptions options, CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(options.VaultFolder))
                Directory.CreateDirectory(options.VaultFolder);
            
            var password = new VaultPassword(PasswordHelper.AskForPassword());
            var vaultCreator = new VaultCreator();
            using var encryptionKey = await vaultCreator.CreateVaultAsync(new NativeFolder(options.VaultFolder), new[] { password }, new VaultOptions
            {
                ContentCipherId = options.ContentCipher switch
                {
                    ContentCipher.AesGcm => Constants.CipherId.AES_GCM,
                    ContentCipher.XChaCha20Poly1305 => Constants.CipherId.XCHACHA20_POLY1305
                },
                FileNameCipherId = options.FileNameCipher switch
                {
                    FileNameCipher.AesSiv => Constants.CipherId.AES_SIV,
                    FileNameCipher.None => Constants.CipherId.NONE
                },
                Specialization = "TODO",
                AuthenticationMethod = "TODO"
            }, cancellationToken);
        }
    }
}