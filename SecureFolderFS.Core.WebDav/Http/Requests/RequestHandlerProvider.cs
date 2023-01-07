using System.Collections.Generic;

namespace SecureFolderFS.Core.WebDav.Http.Requests
{
    internal sealed class RequestHandlerProvider : IRequestHandlerProvider
    {
        private static Dictionary<string, IRequestHandler> InstanceHandlers { get; } = new()
        {
            { "GET", new GetOrHeadHandler() },
            { "HEAD", new GetOrHeadHandler() },
            { "MKCOL", new MkcolHandler() },
            { "PROPFIND", new PropFindHandler() },
            { "PUT", new PutHandler() }
        };

        public IReadOnlyDictionary<string, IRequestHandler> RequestHandlers => InstanceHandlers;

        public RequestHandlerProvider()
        {
            InstanceHandlers.Add("OPTIONS", new OptionsHandler(InstanceHandlers.Keys));
        }
    }
}
