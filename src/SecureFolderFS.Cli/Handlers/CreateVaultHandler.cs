using SecureFolderFS.Cli.Helpers;
using SecureFolderFS.Cli.Options;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.UI.AppModels;
using SecureFolderFS.UI.ServiceImplementation.Vault;
using SecureFolderFS.UI.Storage.NativeStorage;

namespace SecureFolderFS.Cli.Handlers
{
    public sealed class CreateVaultHandler : SingletonBase<CreateVaultHandler>, IHandler<CreateVaultOptions>
    {
        public async Task HandleAsync(CreateVaultOptions options, CancellationToken cancellationToken = default)
        {
            ValidationHelper.ValidateOptions(options);
            if (!Directory.Exists(options.VaultFolder))
                Directory.CreateDirectory(options.VaultFolder);
            
            var password = new VaultPassword(PasswordHelper.AskForPassword());
            var vaultCreator = new VaultCreator();
            using var encryptionKey = await vaultCreator.CreateVaultAsync(new NativeFolder(options.VaultFolder), new[] { password }, new VaultOptions
            {
                ContentCipherId = options.ContentCipher,
                FileNameCipherId = options.FileNameCipher,
                Specialization = "TODO",
                AuthenticationMethod = "TODO"
            }, cancellationToken);
        }
    }
}