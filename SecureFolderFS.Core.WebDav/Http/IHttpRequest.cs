using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SecureFolderFS.Core.WebDav.Http
{
    // TODO: Needs docs
    internal interface IHttpRequest
    {
        Uri? Url { get; }

        string UserAgent { get; }

        string HttpMethod { get; }

        long ContentLength { get; }

        Stream? InputStream { get; }

        Encoding ContentEncoding { get; }

        IReadOnlyDictionary<string, string?> Headers { get; }

        // TODO: Deprecated, use IReadOnlyDictionary
        [Obsolete("Access headers using the dictionary.")]
        IEnumerable<string?> Headers2 { get; }

        [Obsolete("Access headers using the dictionary.")]
        string? GetHeaderValue(string header);
    }
}
