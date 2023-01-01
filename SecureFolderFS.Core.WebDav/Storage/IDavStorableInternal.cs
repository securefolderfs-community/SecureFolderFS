using SecureFolderFS.Sdk.Storage;

namespace SecureFolderFS.Core.WebDav.Storage
{
    internal interface IDavStorableInternal<out TStorableInternal>
        where TStorableInternal : IStorable
    {
        TStorableInternal StorableInternal { get; }
    }
}
