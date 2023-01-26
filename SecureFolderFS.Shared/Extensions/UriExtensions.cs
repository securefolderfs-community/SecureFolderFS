using System;
using System.Linq;

namespace SecureFolderFS.Shared.Extensions
{
    public static class UriExtensions
    {
        public static string GetUriPath(this Uri uri)
        {
            return uri.LocalPath + Uri.UnescapeDataString(uri.Fragment);
        }

        public static Uri GetParentUriPath(this Uri uri)
        {
            return new(uri.AbsoluteUri.Remove(uri.AbsoluteUri.Length - uri.Segments.Last().Length));
        }

        public static string EncodeUriString(this Uri uri)
        {
            return Uri.EscapeDataString(uri.AbsoluteUri);
        }

        public static string GetUriFileName(this Uri uri)
        {
            return uri.Segments.Last();
        }

        public static Uri CombinePath(this Uri baseUri, string path)
        {
            var uriText = baseUri.OriginalString;
            if (uriText.EndsWith("/"))
                uriText = uriText.Substring(0, uriText.Length - 1);

            return new Uri($"{uriText}/{path}", UriKind.Absolute);
        }
    }
}
