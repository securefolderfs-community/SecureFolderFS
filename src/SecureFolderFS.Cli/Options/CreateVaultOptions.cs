using CommandLine;
using SecureFolderFS.Cli.Attributes;
using SecureFolderFS.Core.Cryptography;

namespace SecureFolderFS.Cli.Options
{
    [Verb("create-vault", HelpText = "Create a new vault.")]
    public sealed class CreateVaultOptions
    {
        [Option('v', "vault-folder", Required = true, HelpText = "Path to a folder where the vault should be created. If the specified folder does not exist, it will be created.")]
        public string VaultFolder { get; private set; }
        
        [ValidOptions(Constants.CipherId.AES_GCM, Constants.CipherId.XCHACHA20_POLY1305)]
        [Option("content-cipher", HelpText = $"Valid values: {Constants.CipherId.AES_GCM}, {Constants.CipherId.XCHACHA20_POLY1305} (case insensitive).", Default = Constants.CipherId.XCHACHA20_POLY1305)]
        public string ContentCipher { get; private set; }
        
        [ValidOptions(Constants.CipherId.AES_SIV, Constants.CipherId.NONE)]
        [Option("filename-cipher", HelpText = $"Valid values: {Constants.CipherId.AES_SIV}, {Constants.CipherId.NONE} (case insensitive).", Default = Constants.CipherId.AES_SIV)]
        public string FileNameCipher { get; private set; }
    }
}