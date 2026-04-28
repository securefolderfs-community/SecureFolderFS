using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using static SecureFolderFS.Core.Constants.Vault.Authentication;

namespace SecureFolderFS.Cli.Commands;

[Command("creds change", Description = "Replace an authentication factor.")]
public sealed partial class CredsChangeCommand(IVaultManagerService vaultManagerService, IVaultService vaultService, CredentialReader credentialReader)
    : VaultAuthOptions, ICommand
{
    [CommandParameter(0, Name = "path", Description = "Path to the vault folder.")]
    public required string Path { get; init; }

    [CommandOption("new-password", Description = "Prompt for the replacement password.")]
    public bool NewPassword { get; init; }

    [CommandOption("new-password-stdin", Description = "Read replacement password from stdin.")]
    public bool NewPasswordStdin { get; init; }

    [CommandOption("new-keyfile-generate", Description = "Generate a replacement keyfile at this path.")]
    public string? NewKeyFileGenerate { get; init; }

    [CommandOption("factor", Description = "Factor to replace: primary|2fa.")]
    public string Factor { get; init; } = "primary";

    public override async ValueTask ExecuteAsync(IConsole console)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(RecoveryKey))
            {
                // TODO: verify - recovery-key based re-auth for factor-preserving updates is currently not wired.
                CliOutput.Error(console, this, "--recovery-key is not yet supported for creds change.");
                Environment.ExitCode = CliExitCodes.BadArguments;
                return;
            }

            var vaultFolder = CliCommandHelpers.GetVaultFolder(Path);
            var vaultOptions = await vaultService.GetVaultOptionsAsync(vaultFolder);

            using var auth = await CredentialResolver.ResolveAuthenticationAsync(this, credentialReader);
            if (auth is null)
            {
                CliOutput.Error(console, this, "Current credentials are required.");
                Environment.ExitCode = CliExitCodes.BadArguments;
                return;
            }

            using var unlockContract = await vaultManagerService.UnlockAsync(vaultFolder, auth.Passkey);

            var replacementPassword = await credentialReader.ReadPasswordAsync(NewPassword, NewPasswordStdin, "New password: ", null);
            IKeyUsage? replacement = null;
            string? replacementMethod = null;

            if (!string.IsNullOrWhiteSpace(replacementPassword))
            {
                replacement = new DisposablePassword(replacementPassword);
                replacementMethod = AUTH_PASSWORD;
            }
            else if (!string.IsNullOrWhiteSpace(NewKeyFileGenerate))
            {
                if (string.IsNullOrWhiteSpace(vaultOptions.VaultId))
                    throw new InvalidOperationException("Vault ID is unavailable.");

                replacement = await credentialReader.GenerateKeyFileAsync(NewKeyFileGenerate, vaultOptions.VaultId);
                replacementMethod = AUTH_KEYFILE;
            }

            if (replacement is null || replacementMethod is null)
            {
                CliOutput.Error(console, this, "Provide --new-password/--new-password-stdin or --new-keyfile-generate.");
                Environment.ExitCode = CliExitCodes.BadArguments;
                return;
            }

            using (replacement)
            {
                var methods = vaultOptions.UnlockProcedure.Methods.ToArray();
                var replaceIndex = string.Equals(Factor, "2fa", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
                if (replaceIndex >= methods.Length)
                {
                    CliOutput.Error(console, this, $"Factor '{Factor}' is not configured on this vault.");
                    Environment.ExitCode = CliExitCodes.BadArguments;
                    return;
                }

                methods[replaceIndex] = replacementMethod;
                var updatedOptions = vaultOptions with
                {
                    UnlockProcedure = new AuthenticationMethod(methods, null)
                };

                IKeyUsage updatedPasskey = replacement;
                if (methods.Length > 1)
                {
                    if (auth.Passkey is not KeySequence authSequence || authSequence.Keys.Count < methods.Length)
                        throw new InvalidOperationException("Both authentication factors must be supplied for this vault.");

                    var next = new KeySequence();
                    for (var i = 0; i < methods.Length; i++)
                    {
                        next.Add(i == replaceIndex ? replacement : authSequence.Keys.ElementAt(i));
                    }

                    updatedPasskey = next;
                }

                using (updatedPasskey)
                {
                    await vaultManagerService.ModifyAuthenticationAsync(vaultFolder, unlockContract, updatedPasskey, updatedOptions);
                }
            }

            CliOutput.Success(console, this, $"Credential factor '{Factor}' updated.");
            Environment.ExitCode = CliExitCodes.Success;
        }
        catch (Exception ex)
        {
            Environment.ExitCode = CliCommandHelpers.HandleException(ex, console, this);
        }
    }
}
