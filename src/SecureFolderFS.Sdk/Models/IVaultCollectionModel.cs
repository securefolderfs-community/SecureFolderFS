﻿using SecureFolderFS.Shared.ComponentModel;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Manages the collection of user-saved vaults.
    /// </summary>
    public interface IVaultCollectionModel : ICollection<IVaultModel>, INotifyCollectionChanged, IPersistable
    {
    }
}