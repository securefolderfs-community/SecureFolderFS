using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using static SecureFolderFS.Core.Constants.Vault.Authentication;

namespace SecureFolderFS.Cli.Commands;

[Command("creds add", Description = "Add a second-factor credential to an existing vault.")]
public sealed partial class CredsAddCommand(IVaultManagerService vaultManagerService, IVaultService vaultService, CredentialReader credentialReader)
    : VaultAuthOptions, ICommand
{
    [CommandParameter(0, Name = "path", Description = "Path to the vault folder.")]
    public required string Path { get; init; }

    [CommandOption("twofa-password", Description = "Prompt for a new second-factor password.")]
    public bool NewTwoFactorPassword { get; init; }

    [CommandOption("twofa-password-stdin", Description = "Read new second-factor password from stdin.")]
    public bool NewTwoFactorPasswordStdin { get; init; }

    [CommandOption("twofa-keyfile", Description = "Use an existing second-factor keyfile.")]
    public string? NewTwoFactorKeyFile { get; init; }

    [CommandOption("twofa-keyfile-generate", Description = "Generate a new second-factor keyfile.")]
    public string? NewTwoFactorKeyFileGenerate { get; init; }

    public override async ValueTask ExecuteAsync(IConsole console)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(RecoveryKey))
            {
                // TODO: verify - recovery-key based re-auth for factor-preserving updates is currently not wired.
                CliOutput.Error(console, this, "--recovery-key is not yet supported for creds add.");
                Environment.ExitCode = CliExitCodes.BadArguments;
                return;
            }

            var vaultFolder = CliCommandHelpers.GetVaultFolder(Path);
            var vaultOptions = await vaultService.GetVaultOptionsAsync(vaultFolder);
            if (vaultOptions.UnlockProcedure.Methods.Length > 1)
            {
                CliOutput.Error(console, this, "The vault already has a second factor.");
                Environment.ExitCode = CliExitCodes.BadArguments;
                return;
            }

            using var currentAuth = await CredentialResolver.ResolveAuthenticationAsync(this, credentialReader);
            if (currentAuth is null)
            {
                CliOutput.Error(console, this, "Current credentials are required.");
                Environment.ExitCode = CliExitCodes.BadArguments;
                return;
            }

            using var unlockContract = await vaultManagerService.UnlockAsync(vaultFolder, currentAuth.Passkey);
            var secondPassword = await credentialReader.ReadPasswordAsync(NewTwoFactorPassword, NewTwoFactorPasswordStdin,
                "New second-factor password: ", null);

            IDisposable? secondFactor = null;
            string? secondMethod = null;
            if (!string.IsNullOrWhiteSpace(secondPassword))
            {
                secondFactor = new DisposablePassword(secondPassword);
                secondMethod = AUTH_PASSWORD;
            }
            else if (!string.IsNullOrWhiteSpace(NewTwoFactorKeyFileGenerate))
            {
                if (string.IsNullOrWhiteSpace(vaultOptions.VaultId))
                    throw new InvalidOperationException("Vault ID is unavailable.");

                secondFactor = (IDisposable)await credentialReader.GenerateKeyFileAsync(NewTwoFactorKeyFileGenerate, vaultOptions.VaultId);
                secondMethod = AUTH_KEYFILE;
            }
            else if (!string.IsNullOrWhiteSpace(NewTwoFactorKeyFile))
            {
                secondFactor = (IDisposable)await credentialReader.ReadKeyFileAsKeyAsync(NewTwoFactorKeyFile);
                secondMethod = AUTH_KEYFILE;
            }

            if (secondFactor is not IKeyUsage secondKey || secondMethod is null)
            {
                CliOutput.Error(console, this, "A new second-factor credential is required.");
                Environment.ExitCode = CliExitCodes.BadArguments;
                return;
            }

            using (secondFactor)
            {
                var sequence = new KeySequence();
                sequence.Add(currentAuth.Passkey);
                sequence.Add(secondKey);

                using (sequence)
                {
                    var updatedOptions = vaultOptions with
                    {
                        UnlockProcedure = new AuthenticationMethod([vaultOptions.UnlockProcedure.Methods[0], secondMethod], null)
                    };

                    await vaultManagerService.ModifyAuthenticationAsync(vaultFolder, unlockContract, sequence, updatedOptions);
                }
            }

            CliOutput.Success(console, this, "Second-factor credential added.");
            Environment.ExitCode = CliExitCodes.Success;
        }
        catch (Exception ex)
        {
            Environment.ExitCode = CliCommandHelpers.HandleException(ex, console, this);
        }
    }
}
