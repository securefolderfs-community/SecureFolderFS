using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Cli.Commands;

[Command("vault unmount", Description = "Unmount a mounted vault.")]
public sealed partial class VaultUnmountCommand : CliGlobalOptions, ICommand
{
    [CommandParameter(0, Name = "path", Description = "Vault path or mount path.")]
    public required string Path { get; init; }

    [CommandOption("force", Description = "Force unmount even if files are open.")]
    public bool Force { get; init; }

    public override async ValueTask ExecuteAsync(IConsole console)
    {
        try
        {
            var requestedPath = System.IO.Path.GetFullPath(Path);
            var mounted = FileSystemManager.Instance.FileSystems.ToArray();

            var target = mounted.FirstOrDefault(root => IsMatch(root, requestedPath));
            if (target is null)
            {
                CliOutput.Error(console, this, "No mounted vault matches the provided path.");
                Environment.ExitCode = CliExitCodes.MountStateError;
                return;
            }

            try
            {
                await target.DisposeAsync();
            }
            catch when (Force)
            {
                target.Dispose();
            }

            CliOutput.Success(console, this, "Vault unmounted.");
            Environment.ExitCode = CliExitCodes.Success;
        }
        catch (Exception ex)
        {
            Environment.ExitCode = CliCommandHelpers.HandleException(ex, console, this);
        }
    }

    private static bool IsMatch(IVfsRoot root, string requestedPath)
    {
        if (string.Equals(System.IO.Path.GetFullPath(root.VirtualizedRoot.Id), requestedPath, StringComparison.OrdinalIgnoreCase))
            return true;

        if (root is not IWrapper<FileSystemSpecifics> wrapper)
            return false;

        var contentPath = System.IO.Path.GetFullPath(wrapper.Inner.ContentFolder.Id);
        if (string.Equals(contentPath, requestedPath, StringComparison.OrdinalIgnoreCase))
            return true;

        var parent = Directory.GetParent(contentPath)?.FullName;
        return parent is not null && string.Equals(parent, requestedPath, StringComparison.OrdinalIgnoreCase);
    }
}




