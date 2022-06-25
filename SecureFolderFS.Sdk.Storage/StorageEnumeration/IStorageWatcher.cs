using System;
using SecureFolderFS.Sdk.Storage.EventArguments;

namespace SecureFolderFS.Sdk.Storage.StorageEnumeration
{
    /// <summary>
    /// Watches and notifies of any storage changes.
    /// </summary>
    public interface IStorageWatcher
    {
        /// <summary>
        /// The folder where the watcher is functioning.
        /// </summary>
        IFolder SourceFolder { get; }

        /// <summary>
        /// An event that's fired when a file is deleted.
        /// </summary>
        event EventHandler<PathAffectedEventArgs>? FileDeletedEvent;

        /// <summary>
        /// An event that's fired when a folder is deleted.
        /// </summary>
        event EventHandler<PathAffectedEventArgs>? FolderDeletedEvent;

        /// <summary>
        /// An event that's fired when a file is renamed.
        /// </summary>
        event EventHandler<PathChangedEventArgs>? FileRenamedEvent;

        /// <summary>
        /// An event that's fired when a folder is renamed.
        /// </summary>
        event EventHandler<PathChangedEventArgs>? FolderRenamedEvent;
    }
}
