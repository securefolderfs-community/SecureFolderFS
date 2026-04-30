using System.Runtime.InteropServices;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using SecureFolderFS.Core.WebDav;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Storage.Enums;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Cli.Commands;

[Command("vault mount", Description = "Unlock a vault and mount it as a live filesystem.")]
public sealed partial class VaultMountCommand(IVaultManagerService vaultManagerService, IVaultFileSystemService vaultFileSystemService, CredentialReader credentialReader)
    : VaultAuthOptions, ICommand
{
    [CommandParameter(0, Name = "path", Description = "Path to the vault folder.")]
    public required string Path { get; init; }

    [CommandOption("mount-point", Description = "Filesystem mount point.")]
    public string? MountPoint { get; init; }

    [CommandOption("fs", Description = "Filesystem adapter: auto|webdav|dokany|winfsp.")]
    public string FileSystem { get; init; } = "auto";

    [CommandOption("read-only", Description = "Mount in read-only mode.")]
    public bool ReadOnly { get; init; }

    public override async ValueTask ExecuteAsync(IConsole console)
    {
        IDisposable? unlockContract = null;
        IVfsRoot? mountedRoot = null;

        try
        {
            var usingRecovery = !string.IsNullOrWhiteSpace(RecoveryKey) || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SFFS_RECOVERY_KEY"));
            if (usingRecovery && (Password || PasswordStdin || !string.IsNullOrWhiteSpace(KeyFile) || TwoFactorPassword || TwoFactorPasswordStdin || !string.IsNullOrWhiteSpace(TwoFactorKeyFile)))
            {
                CliOutput.Error(console, this, "--recovery-key is mutually exclusive with credential flags.");
                Environment.ExitCode = CliExitCodes.BadArguments;
                return;
            }

            var vaultFolder = CliCommandHelpers.GetVaultFolder(Path);

            if (usingRecovery)
            {
                var recovery = credentialReader.ReadRecoveryKey(RecoveryKey, Environment.GetEnvironmentVariable("SFFS_RECOVERY_KEY"));
                if (string.IsNullOrWhiteSpace(recovery))
                {
                    CliOutput.Error(console, this, "No recovery key provided.");
                    Environment.ExitCode = CliExitCodes.BadArguments;
                    return;
                }

                unlockContract = await vaultManagerService.RecoverAsync(vaultFolder, recovery);
            }
            else
            {
                using var auth = await CredentialResolver.ResolveAuthenticationAsync(this, credentialReader);
                if (auth is null)
                {
                    CliOutput.Error(console, this, "No credentials provided. Use password/keyfile flags or environment variables.");
                    Environment.ExitCode = CliExitCodes.BadArguments;
                    return;
                }

                unlockContract = await vaultManagerService.UnlockAsync(vaultFolder, auth.Passkey);
            }

            var fileSystem = await ResolveFileSystemAsync(vaultFileSystemService, FileSystem, console);
            var contentFolder = await VaultHelpers.GetContentFolderAsync(vaultFolder);
            var mountOptions = CliCommandHelpers.BuildMountOptions(vaultFolder.Name, ReadOnly, MountPoint);
            mountedRoot = await fileSystem.MountAsync(contentFolder, unlockContract, mountOptions);

            CliOutput.Success(console, this, $"Mounted using {fileSystem.Name}: {mountedRoot.VirtualizedRoot.Id}");

            var shutdownSignal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            ConsoleCancelEventHandler onCancel = (_, args) =>
            {
                args.Cancel = true;
                shutdownSignal.TrySetResult();
            };

            EventHandler onExit = (_, _) => shutdownSignal.TrySetResult();
            Console.CancelKeyPress += onCancel;
            AppDomain.CurrentDomain.ProcessExit += onExit;

            IDisposable? sigTerm = null;
            try
            {
#if NET7_0_OR_GREATER
                sigTerm = PosixSignalRegistration.Create(PosixSignal.SIGTERM, _ => shutdownSignal.TrySetResult());
#endif
                await shutdownSignal.Task;
            }
            finally
            {
                Console.CancelKeyPress -= onCancel;
                AppDomain.CurrentDomain.ProcessExit -= onExit;
                sigTerm?.Dispose();
            }

            await mountedRoot.DisposeAsync();
            unlockContract.Dispose();
            mountedRoot = null;
            unlockContract = null;
            Environment.ExitCode = CliExitCodes.Success;
        }
        catch (NotSupportedException ex)
        {
            CliOutput.Error(console, this, ex.Message);
            Environment.ExitCode = CliExitCodes.MountFailure;
        }
        catch (Exception ex)
        {
            Environment.ExitCode = CliCommandHelpers.HandleException(ex, console, this);
        }
        finally
        {
            if (mountedRoot is not null)
                await mountedRoot.DisposeAsync();

            unlockContract?.Dispose();
        }
    }

    private async Task<IFileSystemInfo> ResolveFileSystemAsync(IVaultFileSystemService service, string requested, IConsole console)
    {
        if (string.Equals(requested, "auto", StringComparison.OrdinalIgnoreCase))
            return await service.GetBestFileSystemAsync();

        var wantedId = requested.ToLowerInvariant() switch
        {
            "webdav" => Constants.FileSystem.FS_ID,
            "dokany" => "DOKANY",
            "winfsp" => "WINFSP",
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(wantedId))
            throw new ArgumentException($"Unknown filesystem '{requested}'.");

        await foreach (var candidate in service.GetFileSystemsAsync())
        {
            if (!string.Equals(candidate.Id, wantedId, StringComparison.OrdinalIgnoreCase))
                continue;

            if (await candidate.GetStatusAsync() == FileSystemAvailability.Available)
                return candidate;

            CliOutput.Warning(console, this, $"Adapter '{requested}' is unavailable. Falling back to auto.");
            return await service.GetBestFileSystemAsync();
        }

        CliOutput.Warning(console, this, $"Adapter '{requested}' not found. Falling back to auto.");
        return await service.GetBestFileSystemAsync();
    }
}
