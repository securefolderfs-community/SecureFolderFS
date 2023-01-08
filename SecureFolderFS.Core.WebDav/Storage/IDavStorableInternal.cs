using NWebDav.Server.Storage;
using SecureFolderFS.Sdk.Storage;

namespace SecureFolderFS.Core.WebDav.Storage
{
    internal interface IDavStorableInternal<out TStorableInternal> : IDavStorable
        where TStorableInternal : IStorable
    {
        TStorableInternal StorableInternal { get; }
    }
}
