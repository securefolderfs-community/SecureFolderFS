using CliFx;
using CliFx.Binding;
using CliFx.Infrastructure;

namespace SecureFolderFS.Cli.Commands;

/// <summary>
/// Lists the vaults known to a running SecureFolderFS instance.
/// </summary>
[Command("app vaults", Description = "List vaults from a running SecureFolderFS instance.")]
public sealed partial class AppVaultsCommand : CliGlobalOptions, ICommand
{
    public override async ValueTask ExecuteAsync(IConsole console)
    {
        try
        {
            await using var client = await AppApiClient.ConnectAsync(console.RegisterCancellationHandler());
            var vaults = await client.ListVaultsAsync();

            if (vaults.Count == 0)
            {
                CliOutput.Info(console, this, "No vaults are visible to integrations.");
                Environment.ExitCode = CliExitCodes.Success;
                return;
            }

            foreach (var vault in vaults)
            {
                var suffix = vault.State == "unlocked" ? $"  {vault.MountPath}" : string.Empty;
                console.Output.WriteLine($"{vault.Id}  {vault.State,-8}  {vault.Name}{suffix}");
            }

            Environment.ExitCode = CliExitCodes.Success;
        }
        catch (Exception ex)
        {
            Environment.ExitCode = CliCommandHelpers.HandleException(ex, console, this);
        }
    }
}

/// <summary>
/// Asks a running SecureFolderFS instance to show its unlock prompt.
/// </summary>
[Command("app unlock", Description = "Ask a running SecureFolderFS instance to prompt for unlocking a vault.")]
public sealed partial class AppUnlockCommand : CliGlobalOptions, ICommand
{
    [CommandParameter(0, Name = "vaultId", Description = "Vault id as reported by 'app vaults'.")]
    public required string VaultId { get; set; }

    public override async ValueTask ExecuteAsync(IConsole console)
    {
        try
        {
            await using var client = await AppApiClient.ConnectAsync(console.RegisterCancellationHandler());
            var status = await client.RequestUnlockAsync(VaultId);

            // The prompt is shown to the user; this command deliberately does not wait for the outcome,
            // because the user may take any amount of time or cancel outright.
            CliOutput.Info(console, this, status switch
            {
                "already_pending" => "An unlock prompt for this vault is already open.",
                "no_change" => "The vault is already unlocked.",
                _ => "SecureFolderFS is prompting for credentials."
            });

            Environment.ExitCode = CliExitCodes.Success;
        }
        catch (Exception ex)
        {
            Environment.ExitCode = CliCommandHelpers.HandleException(ex, console, this);
        }
    }
}

/// <summary>
/// Locks a vault in a running SecureFolderFS instance.
/// </summary>
[Command("app lock", Description = "Lock a vault in a running SecureFolderFS instance.")]
public sealed partial class AppLockCommand : CliGlobalOptions, ICommand
{
    [CommandParameter(0, Name = "vaultId", Description = "Vault id as reported by 'app vaults'.")]
    public required string VaultId { get; set; }

    public override async ValueTask ExecuteAsync(IConsole console)
    {
        try
        {
            await using var client = await AppApiClient.ConnectAsync(console.RegisterCancellationHandler());
            var status = await client.LockAsync(VaultId);

            CliOutput.Info(console, this, status == "no_change"
                ? "The vault is already locked."
                : "Lock requested.");

            Environment.ExitCode = CliExitCodes.Success;
        }
        catch (Exception ex)
        {
            Environment.ExitCode = CliCommandHelpers.HandleException(ex, console, this);
        }
    }
}
