using System;
using System.Security.Principal;

namespace SecureFolderFS.Core.WebDav.Http
{
    /// TODO: Needs docs
    internal interface IHttpSession : IAsyncDisposable
    {
        // TODO: Maybe use IIdentity here?
        IPrincipal? Principal { get; }
    }
}
