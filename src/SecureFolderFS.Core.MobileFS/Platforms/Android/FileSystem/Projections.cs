using static Android.Provider.DocumentsContract;

namespace SecureFolderFS.Core.MobileFS.Platforms.Android.FileSystem
{
    internal static class Projections
    {
        public static string[] DefaultRootProjection { get; } =
        [
            Root.ColumnRootId,
            Root.ColumnIcon,
            Root.ColumnTitle,
            Root.ColumnFlags,
            Root.ColumnDocumentId
        ];

        public static string[] DefaultDocumentProjection { get; } =
        [
            Document.ColumnDocumentId,
            Document.ColumnMimeType,
            Document.ColumnDisplayName,
            Document.ColumnLastModified,
            Document.ColumnFlags,
            Document.ColumnSize
        ];
    }
}
