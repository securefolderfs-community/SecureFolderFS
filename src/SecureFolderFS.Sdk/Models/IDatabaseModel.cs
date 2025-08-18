using System;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Represents a database to store data identified by <typeparamref name="TKey"/>.
    /// </summary>
    /// <typeparam name="TKey">The key to identify data with.</typeparam>
    public interface IDatabaseModel<in TKey> : IPropertyStore<TKey>, IPersistable, IDisposable
    {
    }
}
