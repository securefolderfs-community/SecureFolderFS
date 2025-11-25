using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SecureFolderFS.Storage.VirtualFileSystem
{
    /// <summary>
    /// Specifies file-system-related options to use.
    /// </summary>
    public class FileSystemOptions : INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public virtual event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets whether to use a read-only file system.
        /// </summary>
        public virtual bool IsReadOnly { get; protected set => SetField(ref field, value); }

        /// <summary>
        /// Gets or sets the maximum size in bytes of the recycle bin.
        /// </summary>
        /// <remarks>
        /// If the size is zero, the recycle bin is disabled.
        /// If the size is any value smaller than zero, the recycle bin has unlimited size capacity.
        /// Any values above zero indicate the maximum capacity in bytes that is allowed for the recycling operation to proceed.
        /// </remarks>
        public virtual long RecycleBinSize { get; protected set => SetField(ref field, value); }

        /// <summary>
        /// Updates the specified field and raises the PropertyChanged event if the value has changed.
        /// </summary>
        /// <typeparam name="T">The type of the property being set.</typeparam>
        /// <param name="field">The reference to the field being updated.</param>
        /// <param name="value">The new value to set for the field.</param>
        /// <param name="propertyName">The name of the property being set. This is optional and automatically provided by the caller.</param>
        /// <returns>True if the field value was changed; otherwise, false.</returns>
        protected virtual bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
