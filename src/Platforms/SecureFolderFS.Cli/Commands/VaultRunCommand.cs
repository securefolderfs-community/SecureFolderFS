using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.SystemStorageEx;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Cli.Commands;

[Command("vault run", Description = "Unlock a vault, perform one file operation, then lock.")]
public sealed partial class VaultRunCommand(IVaultManagerService vaultManagerService, IVaultFileSystemService vaultFileSystemService, CredentialReader credentialReader)
    : VaultAuthOptions, ICommand
{
    [CommandParameter(0, Name = "path", Description = "Path to the vault folder.")]
    public required string Path { get; init; }

    [CommandOption("read", Description = "Read a file from the vault.")]
    public string? ReadPath { get; init; }

    [CommandOption("write", Description = "Write stdin to a file in the vault.")]
    public string? WritePath { get; init; }

    [CommandOption("out", Description = "When reading, write output to local file instead of stdout.")]
    public string? OutPath { get; init; }

    public override async ValueTask ExecuteAsync(IConsole console)
    {
        IDisposable? unlockContract = null;
        IVfsRoot? localRoot = null;

        try
        {
            var hasRead = !string.IsNullOrWhiteSpace(ReadPath);
            var hasWrite = !string.IsNullOrWhiteSpace(WritePath);
            if (hasRead == hasWrite)
            {
                CliOutput.Error(console, this, "Exactly one of --read or --write must be specified.");
                Environment.ExitCode = CliExitCodes.BadArguments;
                return;
            }

            var vaultFolder = new SystemFolderEx(Path);
            var recovery = credentialReader.ReadRecoveryKey(RecoveryKey, Environment.GetEnvironmentVariable("SFFS_RECOVERY_KEY"));

            if (!string.IsNullOrWhiteSpace(recovery))
            {
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

            var localFileSystem = await vaultFileSystemService.GetLocalFileSystemAsync();
            var contentFolder = await VaultHelpers.GetContentFolderAsync(vaultFolder);
            var options = CliCommandHelpers.BuildMountOptions(vaultFolder.Name, readOnly: false, mountPoint: null);
            localRoot = await localFileSystem.MountAsync(contentFolder, unlockContract, options);

            if (hasRead)
                await ReadFromVaultAsync(localRoot.PlaintextRoot, ReadPath!, OutPath);
            else
                await WriteToVaultAsync(localRoot.PlaintextRoot, WritePath!);

            Environment.ExitCode = CliExitCodes.Success;
        }
        catch (Exception ex)
        {
            Environment.ExitCode = CliCommandHelpers.HandleException(ex, console, this);
        }
        finally
        {
            if (localRoot is not null)
                await localRoot.DisposeAsync();

            unlockContract?.Dispose();
        }
    }

    private static async Task ReadFromVaultAsync(IFolder root, string path, string? outputPath)
    {
        var item = await root.GetItemByRelativePathAsync(path);
        if (item is not IFile file)
            throw new FileNotFoundException($"Vault file not found: {path}");

        await using var input = await file.OpenReadAsync();
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            await input.CopyToAsync(Console.OpenStandardOutput());
            return;
        }

        var fullPath = System.IO.Path.GetFullPath(outputPath);
        var parent = System.IO.Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(parent))
            Directory.CreateDirectory(parent);

        await using var output = File.Open(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await input.CopyToAsync(output);
    }

    private static async Task WriteToVaultAsync(IFolder root, string path)
    {
        if (root is not IModifiableFolder modifiableRoot)
            throw new InvalidOperationException("The vault is not writable.");

        var normalized = path.Replace('\\', '/').Trim('/');
        var parts = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            throw new ArgumentException("Invalid vault path.");

        IModifiableFolder current = modifiableRoot;
        for (var i = 0; i < parts.Length - 1; i++)
        {
            var existing = await current.TryGetFolderByNameAsync(parts[i]);
            if (existing is IModifiableFolder existingFolder)
            {
                current = existingFolder;
                continue;
            }

            var created = await current.CreateFolderAsync(parts[i], false);
            current = created as IModifiableFolder ?? throw new InvalidOperationException("Created folder is not modifiable.");
        }

        var destination = await current.CreateFileAsync(parts[^1], overwrite: true);
        await using var destinationStream = await destination.OpenWriteAsync();
        await using var stdin = Console.OpenStandardInput();
        await stdin.CopyToAsync(destinationStream);
    }
}
