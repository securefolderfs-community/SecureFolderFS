using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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
                var files = Directory.GetFiles(path);
                Parallel.ForEach(files, () => 0L, (file, _, localTotal) =>
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        return localTotal + fileInfo.Length;
                    }
                    catch
                    {
                        // Ignore errors (e.g., access denied)
                        return localTotal;
                    }
                },
                localTotal => Interlocked.Add(ref totalSize, localTotal));

                // Recurse into subdirectories in parallel
                var subDirs = Directory.GetDirectories(path);
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
            using var configStream = specifics.Options.IsReadOnly ? File.OpenRead(configPath) : (!File.Exists(configPath) ? File.Create(configPath) : File.OpenRead(configPath));
            using var streamReader = new StreamReader(configStream);

            var text = streamReader.ReadToEnd();
            if (!long.TryParse(text, out var value))
                return 0L;

            return Math.Max(0L, value);
        }

        public static void SetOccupiedSize(FileSystemSpecifics specifics, long value)
        {
            if (specifics.Options.IsReadOnly)
                throw FileSystemExceptions.FileSystemReadOnly;

            var configPath = Path.Combine(specifics.ContentFolder.Id, Constants.Names.RECYCLE_BIN_NAME, Constants.Names.RECYCLE_BIN_CONFIGURATION_FILENAME);
            using var configStream = !File.Exists(configPath) ? File.Create(configPath) : File.OpenWrite(configPath);
            using var streamWriter = new StreamWriter(configStream);

            var text = Math.Max(0L, value).ToString();
            streamWriter.Write(text);
        }
    }
}
