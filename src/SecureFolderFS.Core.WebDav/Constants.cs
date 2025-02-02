﻿namespace SecureFolderFS.Core.WebDav
{
    public static class Constants
    {
        public static class FileSystem
        {
            public const string FS_ID = "WEBDAV";
            public const string FS_NAME = "WebDav";
        }

        internal static class Headers
        {
            public const string E_TAG = "Etag";
            public const string LAST_MODIFIED = "Last-Modified";
            public const string CONTENT_TYPE = "Content-Type";
            public const string CONTENT_LANGUAGE = "Content-Language";
            public const string CONTENT_LENGTH = "Content-Length";
            public const string ACCEPT_RANGES = "Accept-Ranges";
            public const string DEPTH = "Depth";
        }

        internal static class WebDavNamespaces
        {
            public const string DAV_NAMESPACE = "Dav:";
            public const string DAV_WIN32_NAMESPACE = "urn:schemas-microsoft-com:";
            public const string DAV_PREFIX_NAMESPACE = "D";
            public const string DAV_WIN32_PREFIX_NAMESPACE = "D";
        }
    }
}
