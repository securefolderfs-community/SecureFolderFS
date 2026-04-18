using System.Runtime.Serialization;
using System.Security.Cryptography;
using CliFx.Infrastructure;
using OwlCore.Storage;
using SecureFolderFS.Core;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.SystemStorageEx;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Cli;

internal static class CliCommandHelpers
{
    public static IFolder GetVaultFolder(string path)
    {
        var fullPath = Path.GetFullPath(path);
        return new SystemFolderEx(fullPath);
    }

    public static Dictionary<string, object> BuildMountOptions(string volumeName, bool readOnly, string? mountPoint)
    {
        var options = new Dictionary<string, object>
        {
            [nameof(VirtualFileSystemOptions.VolumeName)] = FormattingHelpers.SanitizeVolumeName(volumeName, "Vault"),
            [nameof(VirtualFileSystemOptions.IsReadOnly)] = readOnly
        };

        if (!string.IsNullOrWhiteSpace(mountPoint))
            options["MountPoint"] = Path.GetFullPath(mountPoint);

        return options;
    }

    public static VaultOptions BuildVaultOptions(string[] methods, string vaultId, string? contentCipher, string? fileNameCipher)
    {
        return new VaultOptions
        {
            UnlockProcedure = new AuthenticationMethod(methods, null),
            VaultId = vaultId,
            ContentCipherId = string.IsNullOrWhiteSpace(contentCipher)
                ? Core.Cryptography.Constants.CipherId.AES_GCM
                : contentCipher,
            FileNameCipherId = string.IsNullOrWhiteSpace(fileNameCipher)
                ? Core.Cryptography.Constants.CipherId.AES_SIV
                : fileNameCipher,
        };
    }

    public static int HandleException(Exception ex, IConsole console, CliGlobalOptions options)
    {
        switch (ex)
        {
            case CryptographicException:
            case FormatException:
                CliOutput.Error(console, options, ex.Message);
                return CliExitCodes.AuthenticationFailure;

            case FileNotFoundException:
            case DirectoryNotFoundException:
            case SerializationException:
                CliOutput.Error(console, options, ex.Message);
                return CliExitCodes.VaultUnreadable;

            case NotSupportedException:
                CliOutput.Error(console, options, ex.Message);
                return CliExitCodes.MountFailure;

            case InvalidOperationException:
            case ArgumentException:
                CliOutput.Error(console, options, ex.Message);
                return CliExitCodes.BadArguments;

            default:
                CliOutput.Error(console, options, ex.ToString());
                return CliExitCodes.GeneralError;
        }
    }
}


