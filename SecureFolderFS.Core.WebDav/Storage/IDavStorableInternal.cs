using NWebDav.Server.Storage;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Core.WebDav.Storage
{
    // TODO: Add docs
    public interface IDavStorableInternal<out TStorableInternal> : IWrapper<TStorableInternal>, IDavStorable
        where TStorableInternal : IStorable
    {
    }
}
