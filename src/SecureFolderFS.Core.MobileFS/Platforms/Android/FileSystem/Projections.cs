using static Android.Provider.DocumentsContract;

namespace SecureFolderFS.Core.MobileFS.Platforms.Android.FileSystem
{
    internal static class Projections
    {
        public static string[] DefaultRootProjection { get; } =
        [
            Root.ColumnRootId,
            Root.ColumnDocumentId,
            Root.ColumnIcon,
            Root.ColumnTitle,
            Root.ColumnFlags
        ];

        public static string[] DefaultDocumentProjection { get; } =
        [
            Document.ColumnDocumentId,
            Document.ColumnDisplayName,
            Document.ColumnMimeType,
            Document.ColumnSize,
            Document.ColumnFlags
        ];
    }
}
