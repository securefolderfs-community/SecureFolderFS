using CommandLine;

namespace SecureFolderFS.CLI.Options
{
    [Verb("unlock-vault", HelpText = "Unlock a vault.")]
    public sealed class UnlockVaultOptions
    {
        [Option('v', "vault-folder", Required = true, HelpText = "Path to a folder where the vault should be created. If the specified folder does not exist, it will be created.")]
        public string VaultFolder { get; set; }
    }
}