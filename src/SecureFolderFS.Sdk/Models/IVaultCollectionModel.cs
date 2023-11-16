using SecureFolderFS.Shared.Utilities;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Manages the collection of saved vaults.
    /// </summary>
    public interface IVaultCollectionModel : ICollection<IVaultModel>, INotifyCollectionChanged, IPersistable
    {
    }
}