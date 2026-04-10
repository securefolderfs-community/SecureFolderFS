using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Cli.Commands;

[Command("vault info", Description = "Read vault metadata without unlocking.")]
public sealed partial class VaultInfoCommand(IVaultService vaultService) : CliGlobalOptions, ICommand
{
    [CommandParameter(0, Name = "path", Description = "Path to the vault folder.")]
    public required string Path { get; init; }

    public override async ValueTask ExecuteAsync(IConsole console)
    {
        try
        {
            var vaultFolder = CliCommandHelpers.GetVaultFolder(Path);
            var options = await vaultService.GetVaultOptionsAsync(vaultFolder);

            if (!Quiet)
            {
                console.Output.WriteLine($"Vault path: {vaultFolder.Id}");
                console.Output.WriteLine($"Version: {options.Version}");
                console.Output.WriteLine($"Vault ID: {options.VaultId}");
                console.Output.WriteLine($"Content cipher: {options.ContentCipherId}");
                console.Output.WriteLine($"Filename cipher: {options.FileNameCipherId}");
                console.Output.WriteLine($"Name encoding: {options.NameEncodingId}");
                console.Output.WriteLine($"Authentication methods: {string.Join(", ", options.UnlockProcedure.Methods)}");
                console.Output.WriteLine($"2FA configured: {options.UnlockProcedure.Methods.Length > 1}");
            }

            Environment.ExitCode = CliExitCodes.Success;
        }
        catch (Exception ex)
        {
            Environment.ExitCode = CliCommandHelpers.HandleException(ex, console, this);
        }
    }
}




