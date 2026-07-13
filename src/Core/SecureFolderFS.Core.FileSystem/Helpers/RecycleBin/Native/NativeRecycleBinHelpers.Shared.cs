using System;
using System.Collections.Generic;
using System.IO;
using SecureFolderFS.Core.FileSystem.DataModels;
using SecureFolderFS.Core.FileSystem.Helpers.Paths;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.FileSystem.Helpers.RecycleBin.Native
{
    public static partial class NativeRecycleBinHelpers
    {
        /// <summary>
        /// Measures the accumulated plaintext size in bytes of all non-core files inside <paramref name="path"/>.
        /// The directory tree is walked iteratively to avoid unbounded parallelism inside file system callbacks.
        /// </summary>
        public static long GetFolderPlaintextSize(string path, FileSystemSpecifics specifics)
        {
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"The directory '{path}' does not exist.");

            var totalSize = 0L;
            var stack = new Stack<string>();
            stack.Push(path);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                try
                {
                    foreach (var file in Directory.EnumerateFiles(current))
                    {
                        try
                        {
                            if (PathHelpers.IsCoreName(Path.GetFileName(file)))
                                continue;

                            totalSize += CalculatePlaintextSize(new FileInfo(file).Length, specifics);
                        }
                        catch (Exception)
                        {
                            // Ignore
                        }
                    }

                    foreach (var subDirectory in Directory.EnumerateDirectories(current))
                        stack.Push(subDirectory);
                }
                catch (Exception)
                {
                    // Ignore
                }
            }

            return totalSize;
        }

        public static long GetOccupiedSize(FileSystemSpecifics specifics)
        {
            // Reading must not create the configuration file - reads can happen on read-only file systems
            var configPath = Path.Combine(specifics.ContentFolder.Id, Constants.Names.RECYCLE_BIN_NAME, Constants.Names.RECYCLE_BIN_CONFIGURATION_FILENAME);
            if (!File.Exists(configPath))
                return 0L;

            using var configStream = File.Open(configPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var deserialized = StreamSerializer.Instance.TryDeserializeAsync<Stream, RecycleBinDataModel>(configStream).ConfigureAwait(false).GetAwaiter().GetResult();
            if (deserialized is null)
                return 0L;

            return Math.Max(0L, deserialized.OccupiedSize);
        }

        public static void SetOccupiedSize(FileSystemSpecifics specifics, long value)
        {
            if (specifics.Options.IsReadOnly)
                throw FileSystemExceptions.FileSystemReadOnly;

            // File.Create truncates existing content, and so File.OpenWrite must not be used here,
            // because it leaves stale tail bytes (and therefore unparseable JSON) whenever the serialized payload shrinks
            var configPath = Path.Combine(specifics.ContentFolder.Id, Constants.Names.RECYCLE_BIN_NAME, Constants.Names.RECYCLE_BIN_CONFIGURATION_FILENAME);
            using var configStream = File.Create(configPath);
            using var serialized = StreamSerializer.Instance.SerializeAsync(new RecycleBinDataModel()
            {
                OccupiedSize = Math.Max(0L, value)
            }).ConfigureAwait(false).GetAwaiter().GetResult();

            serialized.CopyTo(configStream);
            configStream.Flush();
        }

        internal static long CalculatePlaintextSize(long ciphertextLength, FileSystemSpecifics specifics)
        {
            return Math.Max(0L, specifics.Security.ContentCrypt.CalculatePlaintextSize(
                Math.Max(0L, ciphertextLength - specifics.Security.HeaderCrypt.HeaderCiphertextSize)));
        }
    }
}
