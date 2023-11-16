using System;
using System.Collections.Specialized;

namespace SecureFolderFS.Sdk.Storage.MutableStorage
{
    /// <summary>
    /// A disposable object which can notify of changes to the folder.
    /// </summary>
    public interface IFolderWatcher : INotifyCollectionChanged, IAsyncDisposable, IDisposable
    {
        /// <summary>
        /// Gets the folder being watched for changes.
        /// </summary>
        public IMutableFolder Folder { get; }
    }
}
