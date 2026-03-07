using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.FileSystem.DataModels;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Core.FileSystem.Helpers.RecycleBin.Native
{
    public static partial class NativeRecycleBinHelpers
    {
        public static long GetFolderSizeRecursive(string path)
        {
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"The directory '{path}' does not exist.");

            var totalSize = 0L;
            try
            {
                // Sum file sizes in the current directory
                var files = Directory.EnumerateFiles(path);
                Parallel.ForEach(files, () => 0L, (file, _, localTotal) =>
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        return localTotal + fileInfo.Length;
                    }
                    catch (Exception)
                    {
                        // Ignore errors (e.g., access denied)
                        return localTotal;
                    }
                },
                localTotal => Interlocked.Add(ref totalSize, localTotal));

                // Recurse into subdirectories in parallel
                var subDirs = Directory.EnumerateDirectories(path);
                Parallel.ForEach(subDirs, dir =>
                {
                    var subDirSize = GetFolderSizeRecursive(dir);
                    Interlocked.Add(ref totalSize, subDirSize);
                });
            }
            catch (Exception)
            {
            }

            return totalSize;
        }

        public static long GetOccupiedSize(FileSystemSpecifics specifics)
        {
            var configPath = Path.Combine(specifics.ContentFolder.Id, Constants.Names.RECYCLE_BIN_NAME, Constants.Names.RECYCLE_BIN_CONFIGURATION_FILENAME);
            using var configStream = File.Open(configPath, specifics.Options.IsReadOnly ? FileMode.Open : FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);

            var deserialized = StreamSerializer.Instance.TryDeserializeAsync<Stream, RecycleBinDataModel>(configStream).ConfigureAwait(false).GetAwaiter().GetResult();
            if (deserialized is null)
                return 0L;

            return Math.Max(0L, deserialized.OccupiedSize);
        }

        public static void SetOccupiedSize(FileSystemSpecifics specifics, long value)
        {
            if (specifics.Options.IsReadOnly)
                throw FileSystemExceptions.FileSystemReadOnly;

            var configPath = Path.Combine(specifics.ContentFolder.Id, Constants.Names.RECYCLE_BIN_NAME, Constants.Names.RECYCLE_BIN_CONFIGURATION_FILENAME);
            using var configStream = !File.Exists(configPath) ? File.Create(configPath) : File.OpenWrite(configPath);
            using var serialized = StreamSerializer.Instance.SerializeAsync(new RecycleBinDataModel()
            {
                OccupiedSize = Math.Max(0L, value)
            }).ConfigureAwait(false).GetAwaiter().GetResult();

            serialized.CopyTo(configStream);
            configStream.Flush();
        }
    }
}
