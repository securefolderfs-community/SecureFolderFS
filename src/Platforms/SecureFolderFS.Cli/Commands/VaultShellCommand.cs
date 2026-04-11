using System.Text;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Cli.Commands;

[Command("vault shell", Description = "Unlock a vault and enter an interactive shell.")]
public sealed partial class VaultShellCommand(IVaultManagerService vaultManagerService, IVaultFileSystemService vaultFileSystemService, CredentialReader credentialReader)
    : VaultAuthOptions, ICommand
{
    [CommandParameter(0, Name = "path", Description = "Path to the vault folder.")]
    public required string Path { get; init; }

    public override async ValueTask ExecuteAsync(IConsole console)
    {
        IDisposable? unlockContract = null;
        IVfsRoot? localRoot = null;

        try
        {
            var vaultFolder = CliCommandHelpers.GetVaultFolder(Path);
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

            await RunShellAsync(localRoot.PlaintextRoot);
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

    private static async Task RunShellAsync(IFolder root)
    {
        if (root is not IModifiableFolder modifiableRoot)
            throw new InvalidOperationException("The vault is not writable.");

        var current = root;

        while (true)
        {
            Console.Write($"sffs:{current.Id}> ");
            var line = await Console.In.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var args = Tokenize(line);
            var command = args[0].ToLowerInvariant();

            switch (command)
            {
                case "exit":
                    return;

                case "help":
                    Console.WriteLine("ls [path], cd <path>, cat <path>, cp <src> <dst>, mv <src> <dst>, rm <path> [-r], mkdir <path>, pwd, help, exit");
                    break;

                case "pwd":
                    Console.WriteLine(current.Id);
                    break;

                case "ls":
                {
                    var target = args.Count > 1 ? await ResolveFolderAsync(root, current, args[1]) : current;
                    await foreach (var item in target.GetItemsAsync())
                        Console.WriteLine($"{item.Name}\t\t{(item is IFolder ? "[Directory]" : "[File]")}");
                    break;
                }

                case "cd":
                    if (args.Count < 2)
                        throw new ArgumentException("cd requires a path.");

                    current = await ResolveFolderAsync(root, current, args[1]);
                    break;

                case "cat":
                    if (args.Count < 2)
                        throw new ArgumentException("cat requires a path.");

                    var catItem = await ResolveStorableAsync(root, current, args[1]);
                    if (catItem is not IFile catFile)
                        throw new FileNotFoundException("The selected path is not a file.");

                    await using (var input = await catFile.OpenReadAsync())
                    {
                        await input.CopyToAsync(Console.OpenStandardOutput());
                    }
                    Console.WriteLine();
                    break;

                case "mkdir":
                    if (args.Count < 2)
                        throw new ArgumentException("mkdir requires a path.");

                    _ = await EnsureFolderAsync(modifiableRoot, current, args[1]);
                    break;

                case "rm":
                    if (args.Count < 2)
                        throw new ArgumentException("rm requires a path.");

                    var recursive = args.Count > 2 && string.Equals(args[2], "-r", StringComparison.OrdinalIgnoreCase);
                    var toDelete = await ResolveStorableAsync(root, current, args[1]);
                    if (toDelete is IFolder && !recursive)
                        throw new InvalidOperationException("Use -r to remove directories.");

                    if (toDelete is IStorableChild child)
                        await modifiableRoot.DeleteAsync(child, deleteImmediately: true);
                    break;

                case "cp":
                    if (args.Count < 3)
                        throw new ArgumentException("cp requires source and destination.");

                    await CopyAsync(modifiableRoot, root, current, args[1], args[2]);
                    break;

                case "mv":
                    if (args.Count < 3)
                        throw new ArgumentException("mv requires source and destination.");

                    await MoveAsync(modifiableRoot, root, current, args[1], args[2]);
                    break;

                default:
                    Console.WriteLine($"Unknown command '{command}'. Type 'help'.");
                    break;
            }
        }
    }

    private static async Task CopyAsync(IModifiableFolder modifiableRoot, IFolder root, IFolder current, string src, string dst)
    {
        var srcLocal = TryParseLocal(src, out var srcLocalPath);
        var dstLocal = TryParseLocal(dst, out var dstLocalPath);

        if (srcLocal && dstLocal)
        {
            File.Copy(srcLocalPath!, dstLocalPath!, overwrite: true);
            return;
        }

        if (srcLocal)
        {
            var destinationFile = await CreateOrGetVaultFileAsync(modifiableRoot, current, dst);
            await using var inStream = File.OpenRead(srcLocalPath!);
            await using var outStream = await destinationFile.OpenWriteAsync();
            await inStream.CopyToAsync(outStream);
            return;
        }

        var sourceItem = await ResolveStorableAsync(root, current, src);
        if (dstLocal)
        {
            if (sourceItem is not IFile sourceFile)
                throw new InvalidOperationException("Only file copies are supported for vault -> local.");

            var parent = System.IO.Path.GetDirectoryName(dstLocalPath!);
            if (!string.IsNullOrWhiteSpace(parent))
                Directory.CreateDirectory(parent);

            await using var inStream = await sourceFile.OpenReadAsync();
            await using var outStream = File.Open(dstLocalPath!, FileMode.Create, FileAccess.Write, FileShare.None);
            await inStream.CopyToAsync(outStream);
            return;
        }

        var (destinationFolder, destinationName) = await ResolveDestinationFolderAsync(modifiableRoot, current, dst);
        _ = await destinationFolder.CreateCopyOfStorableAsync((IStorable)sourceItem, overwrite: true, destinationName, reporter: null);
    }

    private static async Task MoveAsync(IModifiableFolder modifiableRoot, IFolder root, IFolder current, string src, string dst)
    {
        var srcLocal = TryParseLocal(src, out var srcLocalPath);
        var dstLocal = TryParseLocal(dst, out var dstLocalPath);

        if (srcLocal && dstLocal)
        {
            File.Move(srcLocalPath!, dstLocalPath!, overwrite: true);
            return;
        }

        if (srcLocal || dstLocal)
        {
            // TODO: verify - cross-provider mv semantics are currently copy+delete.
            await CopyAsync(modifiableRoot, root, current, src, dst);
            if (srcLocal)
                File.Delete(srcLocalPath!);
            else if (await ResolveStorableAsync(root, current, src) is IStorableChild child)
                await modifiableRoot.DeleteAsync(child, deleteImmediately: true);

            return;
        }

        var sourceItem = await ResolveStorableAsync(root, current, src);
        if (sourceItem is not IStorableChild sourceChild)
            throw new InvalidOperationException("Source item is not movable.");

        var sourceParent = await ResolveParentFolderAsync(modifiableRoot, root, current, src);
        var (destinationFolder, destinationName) = await ResolveDestinationFolderAsync(modifiableRoot, current, dst);
        _ = await destinationFolder.MoveStorableFromAsync(sourceChild, sourceParent, overwrite: true, destinationName, reporter: null);
    }

    private static async Task<IModifiableFolder> ResolveParentFolderAsync(IModifiableFolder root, IFolder absoluteRoot, IFolder current, string path)
    {
        var normalized = NormalizeVaultPath(path);
        var parentPath = normalized.Contains('/') ? normalized[..normalized.LastIndexOf('/')] : string.Empty;
        if (string.IsNullOrWhiteSpace(parentPath))
            return current as IModifiableFolder ?? root;

        var folder = await ResolveFolderAsync(absoluteRoot, current, parentPath);
        return folder as IModifiableFolder ?? throw new InvalidOperationException("Parent folder is not writable.");
    }

    private static async Task<(IModifiableFolder folder, string name)> ResolveDestinationFolderAsync(IModifiableFolder root, IFolder current, string path)
    {
        var normalized = NormalizeVaultPath(path);
        var split = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (split.Length == 0)
            throw new ArgumentException("Invalid destination path.");

        var destinationName = split[^1];
        var parent = split.Length == 1 ? string.Empty : string.Join('/', split[..^1]);

        var folder = await EnsureFolderAsync(root, current, parent);
        return (folder, destinationName);
    }

    private static async Task<IChildFile> CreateOrGetVaultFileAsync(IModifiableFolder root, IFolder current, string path)
    {
        var (folder, name) = await ResolveDestinationFolderAsync(root, current, path);
        return await folder.CreateFileAsync(name, overwrite: true);
    }

    private static async Task<IModifiableFolder> EnsureFolderAsync(IModifiableFolder root, IFolder current, string path)
    {
        var normalized = NormalizeVaultPath(path);
        if (string.IsNullOrWhiteSpace(normalized))
            return current as IModifiableFolder ?? root;

        var parts = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
        IModifiableFolder folder = normalized.StartsWith('/') ? root : current as IModifiableFolder ?? root;

        foreach (var part in parts)
        {
            var next = await folder.TryGetFolderByNameAsync(part);
            if (next is IModifiableFolder nextFolder)
            {
                folder = nextFolder;
                continue;
            }

            var created = await folder.CreateFolderAsync(part, overwrite: false);
            folder = created as IModifiableFolder ?? throw new InvalidOperationException("Created folder is not modifiable.");
        }

        return folder;
    }

    private static async Task<IStorable> ResolveStorableAsync(IFolder root, IFolder current, string path)
    {
        var normalized = NormalizeVaultPath(path);
        if (normalized.StartsWith('/'))
            return await root.GetItemByRelativePathAsync(normalized.TrimStart('/'));

        return await current.GetItemByRelativePathAsync(normalized);
    }

    private static async Task<IFolder> ResolveFolderAsync(IFolder root, IFolder current, string path)
    {
        var item = await ResolveStorableAsync(root, current, path);
        return item as IFolder ?? throw new InvalidOperationException("Path is not a folder.");
    }

    private static string NormalizeVaultPath(string path)
    {
        return path.Replace('\\', '/').Trim();
    }

    private static bool TryParseLocal(string value, out string? localPath)
    {
        if (value.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
        {
            localPath = System.IO.Path.GetFullPath(value[7..]);
            return true;
        }

        localPath = null;
        return false;
    }

    private static List<string> Tokenize(string input)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        foreach (var ch in input)
        {
            if (ch == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (char.IsWhiteSpace(ch) && !inQuotes)
            {
                if (current.Length > 0)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }

                continue;
            }

            current.Append(ch);
        }

        if (current.Length > 0)
            result.Add(current.ToString());

        return result;
    }
}
