using SecureFolderFS.Shared.ComponentModel;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Manages the collection of user-saved vaults.
    /// </summary>
    public interface IVaultCollectionModel : IList<IVaultModel>, INotifyCollectionChanged, IPersistable
    {
        /// <summary>
        /// Move item at <paramref name="oldIndex"/> to <paramref name="newIndex"/>.
        /// </summary>
        /// <param name="oldIndex">The zero-based index specifying the location of the item to be moved.</param>
        /// <param name="newIndex">The zero-based index specifying the new location of the item.</param>
        void Move(int oldIndex, int newIndex);
    }
}