using System.Collections.Generic;

namespace SecureFolderFS.Core.WebDav.Http
{
    // TODO: Needs docs
    internal interface IRequestHandlerProvider
    {
        IReadOnlyDictionary<string, IRequestHandler> RequestHandlers { get; }
    }
}
