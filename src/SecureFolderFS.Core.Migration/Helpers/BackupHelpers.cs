using OwlCore.Storage;
using SecureFolderFS.Storage.Extensions;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Migration.Helpers
{
    internal static class BackupHelpers
    {
        public static async Task CreateBackup(IModifiableFolder folder, string originalName, int version, Stream stream, CancellationToken cancellationToken)
        {
            var backupName = $"{originalName}_{version}.bkup";
            var backupFile = await folder.CreateFileAsync(backupName, true, cancellationToken);
            await using var backupStream = await backupFile.OpenWriteAsync(cancellationToken);

            await stream.CopyToAsync(backupStream, cancellationToken);
            stream.Position = 0L;
        }

        public static async Task CreateBackup(IModifiableFolder folder, string originalName, int version, CancellationToken cancellationToken)
        {
            var backupName = $"{originalName}_{version}.bkup";
            var backupFile = await folder.CreateFileAsync(backupName, true, cancellationToken);
            var originalFile = await folder.GetFileByNameAsync(originalName, cancellationToken);

            await originalFile.CopyContentsToAsync(backupFile, cancellationToken);
        }
    }
}
