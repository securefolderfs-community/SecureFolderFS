using SecureFolderFS.Shared.Extensions;
using System;
using System.Collections.Generic;

namespace SecureFolderFS.Storage.VirtualFileSystem
{
    /// <inheritdoc cref="FileSystemOptions"/>
    public class VirtualFileSystemOptions : FileSystemOptions
    {
        /// <summary>
        /// Gets or sets the name to use for the volume.
        /// </summary>
        public virtual required string VolumeName { get; init; }

        /// <summary>
        /// Gets or sets the instance file system health statistics which are reported by the underlying virtual file system.
        /// </summary>
        public virtual required IHealthStatistics HealthStatistics { get; init; }

        /// <summary>
        /// Gets or sets the instance file system statistics which are reported by the underlying virtual file system.
        /// </summary>
        public virtual required IFileSystemStatistics FileSystemStatistics { get; init; }

        /// <summary>
        /// Gets or sets whether to enable caching for decrypted content chunks.
        /// </summary>
        public virtual bool IsCachingChunks { get; protected set => SetField(ref field, value); } = true;

        /// <summary>
        /// Gets or sets whether to enable caching for Directory IDs.
        /// </summary>
        public virtual bool IsCachingDirectoryIds { get; protected set => SetField(ref field, value); } = true;

        /// <summary>
        /// Gets or sets whether to enable caching for ciphertext and plaintext names.
        /// </summary>
        public virtual bool IsCachingFileNames { get; protected set => SetField(ref field, value); } = true;

        /// <summary>
        /// Sets the read-only status of the file system.
        /// </summary>
        /// <param name="value">If true, sets the file system to read-only mode; otherwise, sets it to read-write mode.</param>
        public virtual void DangerousSetReadOnly(bool value)
        {
            IsReadOnly = value;
        }

        /// <summary>
        /// Sets the maximum size of the recycle bin.
        /// </summary>
        /// <param name="value">The size in bytes.</param>
        public virtual void DangerousSetRecycleBin(long value)
        {
            RecycleBinSize = value;
        }

        /// <summary>
        /// Gets an optional, detailed description of the file system-specific information to display for the user.
        /// </summary>
        /// <returns>If available, returns a short description about the file system specifics; otherwise null.</returns>
        public virtual string? GetDescription()
        {
            return null;
        }

        /// <summary>
        /// Converts a dictionary of options to a <see cref="VirtualFileSystemOptions"/> instance.
        /// </summary>
        /// <param name="options">The dictionary of options.</param>
        /// <param name="healthStatistics">The function to get health statistics.</param>
        /// <param name="fileSystemStatistics">The function to get file system statistics.</param>
        /// <returns>A <see cref="VirtualFileSystemOptions"/> instance.</returns>
        public static VirtualFileSystemOptions ToOptions(
            IDictionary<string, object> options,
            Func<IHealthStatistics> healthStatistics,
            Func<IFileSystemStatistics> fileSystemStatistics)
        {
            return new VirtualFileSystemOptions
            {
                VolumeName = GetOption<string>(options, nameof(VolumeName)) ?? throw new ArgumentNullException(nameof(VolumeName)),
                HealthStatistics = GetOption<IHealthStatistics>(options, nameof(HealthStatistics)) ?? healthStatistics.Invoke(),
                FileSystemStatistics = GetOption<IFileSystemStatistics>(options, nameof(FileSystemStatistics)) ?? fileSystemStatistics.Invoke(),
                IsReadOnly = GetOption<bool?>(options, nameof(IsReadOnly)) ?? false,
                IsCachingChunks = GetOption<bool?>(options, nameof(IsCachingChunks)) ?? true,
                IsCachingFileNames = GetOption<bool?>(options, nameof(IsCachingFileNames)) ?? true,
                IsCachingDirectoryIds = GetOption<bool?>(options, nameof(IsCachingDirectoryIds)) ?? true,
                RecycleBinSize = GetOption<long?>(options, nameof(RecycleBinSize)) ?? 0L
            };
        }

        /// <summary>
        /// Gets an option from the dictionary.
        /// </summary>
        /// <typeparam name="T">The type of the option.</typeparam>
        /// <param name="options">The dictionary of options.</param>
        /// <param name="name">The name of the option.</param>
        /// <returns>The option value if found; otherwise, the default value of <typeparamref name="T"/>.</returns>
        private static T? GetOption<T>(IDictionary<string, object> options, string name)
        {
            return options.Get(name) is T value ? value : default;
        }
    }
}
