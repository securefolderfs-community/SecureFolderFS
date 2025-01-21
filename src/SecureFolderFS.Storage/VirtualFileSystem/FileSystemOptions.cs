using SecureFolderFS.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SecureFolderFS.Storage.VirtualFileSystem
{
    /// <summary>
    /// Specifies file system related options to use.
    /// </summary>
    public class FileSystemOptions : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the name to use for the volume.
        /// </summary>
        public required string VolumeName { get; init; }

        /// <summary>
        /// Gets or sets the instance file system health statistics which are reported by the underlying virtual file system.
        /// </summary>
        public required IHealthStatistics HealthStatistics { get; init; }

        /// <summary>
        /// Gets or sets the instance file system statistics which are reported by the underlying virtual file system.
        /// </summary>
        public required IFileSystemStatistics FileSystemStatistics { get; init; }

        /// <summary>
        /// Gets or sets whether to use a read-only file system.
        /// </summary>
        public bool IsReadOnly { get; protected set => SetField(ref field, value); }

        /// <summary>
        /// Gets or sets whether to enable caching for decrypted content chunks.
        /// </summary>
        public bool IsCachingChunks { get; protected set => SetField(ref field, value); } = true;

        /// <summary>
        /// Gets or sets whether to enable caching for ciphertext and plaintext names.
        /// </summary>
        public bool IsCachingFileNames { get; protected set => SetField(ref field, value); } = true;
        
        /// <summary>
        /// Gets or sets whether to use recycle bin for the vault.
        /// </summary>
        public bool IsRecycleBinEnabled { get; protected set => SetField(ref field, value); }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Sets the read-only status of the file system.
        /// </summary>
        /// <param name="value">If true, sets the file system to read-only mode; otherwise, sets it to read-write mode.</param>
        public void DangerousSetReadOnly(bool value)
        {
            IsReadOnly = value;
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
        /// Converts a dictionary of options to a <see cref="FileSystemOptions"/> instance.
        /// </summary>
        /// <param name="options">The dictionary of options.</param>
        /// <param name="healthStatistics">The function to get health statistics.</param>
        /// <param name="fileSystemStatistics">The function to get file system statistics.</param>
        /// <returns>A <see cref="FileSystemOptions"/> instance.</returns>
        public static FileSystemOptions ToOptions(
            IDictionary<string, object> options,
            Func<IHealthStatistics> healthStatistics,
            Func<IFileSystemStatistics> fileSystemStatistics)
        {
            return new FileSystemOptions
            {
                VolumeName = GetOption<string>(options, nameof(VolumeName)) ?? throw new ArgumentNullException(nameof(VolumeName)),
                HealthStatistics = GetOption<IHealthStatistics>(options, nameof(HealthStatistics)) ?? healthStatistics.Invoke(),
                FileSystemStatistics = GetOption<IFileSystemStatistics>(options, nameof(FileSystemStatistics)) ?? fileSystemStatistics.Invoke(),
                IsReadOnly = GetOption<bool?>(options, nameof(IsReadOnly)) ?? false,
                IsCachingChunks = GetOption<bool?>(options, nameof(IsCachingChunks)) ?? true,
                IsCachingFileNames = GetOption<bool?>(options, nameof(IsCachingFileNames)) ?? true,
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

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
