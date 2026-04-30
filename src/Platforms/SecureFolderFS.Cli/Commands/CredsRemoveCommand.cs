using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Cli.Commands;

[Command("creds remove", Description = "Remove the second-factor credential from a vault.")]
public sealed partial class CredsRemoveCommand(IVaultManagerService vaultManagerService, IVaultService vaultService, CredentialReader credentialReader)
    : VaultAuthOptions, ICommand
{
    [CommandParameter(0, Name = "path", Description = "Path to the vault folder.")]
    public required string Path { get; init; }

    public override async ValueTask ExecuteAsync(IConsole console)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(RecoveryKey))
            {
                // TODO: verify - recovery-key based re-auth for factor-preserving updates is currently not wired.
                CliOutput.Error(console, this, "--recovery-key is not yet supported for creds remove.");
                Environment.ExitCode = CliExitCodes.BadArguments;
                return;
            }

            var vaultFolder = CliCommandHelpers.GetVaultFolder(Path);
            var vaultOptions = await vaultService.GetVaultOptionsAsync(vaultFolder);
            if (vaultOptions.UnlockProcedure.Methods.Length <= 1)
            {
                CliOutput.Error(console, this, "The vault has no second factor to remove.");
                Environment.ExitCode = CliExitCodes.BadArguments;
                return;
            }

            using var auth = await CredentialResolver.ResolveAuthenticationAsync(this, credentialReader);
            if (auth is null)
            {
                CliOutput.Error(console, this, "Current credentials are required.");
                Environment.ExitCode = CliExitCodes.BadArguments;
                return;
            }

            using var unlockContract = await vaultManagerService.UnlockAsync(vaultFolder, auth.Passkey);
            var primaryOnlyOptions = vaultOptions with
            {
                UnlockProcedure = new AuthenticationMethod([vaultOptions.UnlockProcedure.Methods[0]], null)
            };

            // TODO: verify - assumes the first item in the current auth chain corresponds to the primary factor.
            IKeyUsage newPrimary = auth.Passkey;
            if (auth.Passkey is KeySequence sequence && sequence.Keys.FirstOrDefault() is IKeyUsage key)
                newPrimary = key;

            await vaultManagerService.ModifyAuthenticationAsync(vaultFolder, unlockContract, newPrimary, primaryOnlyOptions);

            CliOutput.Success(console, this, "Second-factor credential removed.");
            Environment.ExitCode = CliExitCodes.Success;
        }
        catch (Exception ex)
        {
            Environment.ExitCode = CliCommandHelpers.HandleException(ex, console, this);
        }
    }
}
