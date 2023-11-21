using CommandLine;
using SecureFolderFS.Cli.Enums;

namespace SecureFolderFS.Cli.Options
{
    [Verb("create-vault", HelpText = "Create a new vault.")]
    public sealed class CreateVaultOptions
    {
        [Option('v', "vault-folder", Required = true, HelpText = "Path to a folder where the vault should be created. If the specified folder does not exist, it will be created.")]
        public string VaultFolder { get; private set; }
        
        [Option("content-cipher", Default = ContentCipher.XChaCha20Poly1305)]
        public ContentCipher ContentCipher { get; private set; }
        
        [Option("filename-cipher", Default = FileNameCipher.AesSiv)]
        public FileNameCipher FileNameCipher { get; private set; }
    }
}