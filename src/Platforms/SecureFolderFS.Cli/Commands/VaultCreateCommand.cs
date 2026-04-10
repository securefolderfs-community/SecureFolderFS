using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using OwlCore.Storage;
using SecureFolderFS.Core;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Storage.Extensions;

namespace SecureFolderFS.Cli.Commands;

[Command("vault create", Description = "Create a new vault at the specified path.")]
public sealed partial class VaultCreateCommand(IVaultManagerService vaultManagerService, CredentialReader credentialReader) : CreateAuthOptions, ICommand
{
    [CommandParameter(0, Name = "path", Description = "Path to the vault folder.")]
    public required string Path { get; init; }

    [CommandOption("name", Description = "Display name for the vault.")]
    public string? Name { get; init; }

    [CommandOption("content-cipher", Description = "Content cipher id (for example: AES-GCM, XChaCha20-Poly1305, none).")]
    public string? ContentCipher { get; init; }

    [CommandOption("filename-cipher", Description = "Filename cipher id (for example: AES-SIV, none).")]
    public string? FileNameCipher { get; init; }

    [CommandOption("overwrite", Description = "Allow creation when a vault already exists.")]
    public bool Overwrite { get; init; }

    public override async ValueTask ExecuteAsync(IConsole console)
    {
        try
        {
            var vaultFolder = CliCommandHelpers.GetVaultFolder(Path);
            var modifiableFolder = vaultFolder as IModifiableFolder;
            if (modifiableFolder is null)
            {
                CliOutput.Error(console, this, "The vault folder is not writable.");
                Environment.ExitCode = CliExitCodes.BadArguments;
                return;
            }

            var existingConfig = await vaultFolder.TryGetFirstByNameAsync(Constants.Vault.Names.VAULT_CONFIGURATION_FILENAME);
            if (existingConfig is not null && !Overwrite)
            {
                CliOutput.Error(console, this, "A vault already exists at this path. Use --overwrite to continue.");
                Environment.ExitCode = CliExitCodes.BadArguments;
                return;
            }

            var vaultId = Guid.NewGuid().ToString("N");
            using var auth = await CredentialResolver.ResolveCreateAuthenticationAsync(this, credentialReader, vaultId);
            if (auth is null)
            {
                CliOutput.Error(console, this, "At least one primary credential is required.");
                Environment.ExitCode = CliExitCodes.BadArguments;
                return;
            }

            var vaultOptions = CliCommandHelpers.BuildVaultOptions(auth.Methods, vaultId, ContentCipher, FileNameCipher);
            using var recoveryKey = await vaultManagerService.CreateAsync(vaultFolder, auth.Passkey, vaultOptions);

            var displayName = string.IsNullOrWhiteSpace(Name) ? System.IO.Path.GetFileName(vaultFolder.Id) : Name;
            CliOutput.Success(console, this, $"Vault created: {displayName}");
            console.Output.WriteLine($"Recovery key: {recoveryKey}");
            Environment.ExitCode = CliExitCodes.Success;
        }
        catch (Exception ex)
        {
            Environment.ExitCode = CliCommandHelpers.HandleException(ex, console, this);
        }
    }
}




