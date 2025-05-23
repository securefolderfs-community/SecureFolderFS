using System.IO;
using System.Linq;
using MimeTypes;
using OwlCore.Storage;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Enums;

namespace SecureFolderFS.Sdk.Helpers
{
    public static class FileTypeHelper
    {
        public static TypeClassification GetClassification(IStorable storable)
        {
            var mimeType = MimeTypeMap.GetMimeType(storable.Id);
            var extension = Path.GetExtension(storable.Id);
            var typeHint = GetTypeFromMime(mimeType);
            typeHint = typeHint == TypeHint.Unclassified ? GetTypeFromExtension(Path.GetExtension(storable.Id)) : typeHint;

            return new(mimeType, typeHint, extension);
        }

        public static TypeHint GetType(IStorable storable)
        {
            var mimeType = MimeTypeMap.GetMimeType(storable.Id);
            var typeHint = GetTypeFromMime(mimeType);

            return typeHint == TypeHint.Unclassified ? GetTypeFromExtension(Path.GetExtension(storable.Id)) : typeHint;
        }

        public static TypeHint GetTypeFromMime(string mimeType)
        {
            return Image()
                   ?? Plaintext()
                   ?? Document()
                   ?? Media()
                   ?? TypeHint.Unclassified;

            TypeHint? Media()
            {
                return mimeType.Equals("video/x-msvideo")
                    || mimeType.Equals("video/mp4")
                    || mimeType.Equals("video/mpeg")
                    || mimeType.Equals("video/ogg")
                    || mimeType.Equals("video/webm")
                    || mimeType.Equals("video/3gpp")
                    || mimeType.Equals("video/3gpp2")

                    ? TypeHint.Media : null;
            }

            TypeHint? Document()
            {
                return mimeType.Equals("application/pdf")
                    || mimeType.Equals("text/csv")
                    || mimeType.Equals("application/msword")
                    || mimeType.Equals("application/vnd.openxmlformats-officedocument.wordprocessingml.document")

                    ? TypeHint.Document : null;
            }

            TypeHint? Plaintext()
            {
                return mimeType.StartsWith("text/")
                    && !mimeType.Equals("text/csv")
                    //|| mimeType.StartsWith("")

                    ? TypeHint.Plaintext : null;
            }

            TypeHint? Image()
            {
                return mimeType.StartsWith("image/")
                    //|| mimeType.StartsWith("")

                    ? TypeHint.Image : null;
            }
        }

        public static TypeHint GetTypeFromExtension(string extension)
        {
            if (!extension.StartsWith('.'))
                extension = $".{extension}";

            // PlainText (code)
            if (CodeExtensions.Contains(extension))
                return TypeHint.Plaintext;

            // PlainText (text)
            if (TextExtensions.Contains(extension))
                return TypeHint.Plaintext;

            // Document
            if (DocumentExtensions.Contains(extension))
                return TypeHint.Document;

            // Image
            if (ImageExtensions.Contains(extension))
                return TypeHint.Image;

            // Media
            if (MediaExtensions.Contains(extension))
                return TypeHint.Media;

            // Audio
            if (AudioExtensions.Contains(extension))
                return TypeHint.Audio;

            return TypeHint.Unclassified;
        }

        public static string[] CodeExtensions { get; } =
        {
            // Low level languages
            ".cpp", ".c", ".cxx",
            ".h", ".def", ".pl",
            ".rs",

            // Common languages
            ".java", ".cs", ".py",

            // Web languages
            ".htm", ".html", ".js",
            ".css", ".svelte", ".php",
            ".scss", ".ts",

            // Other
            ".bat", ".xml", ".json",
            ".inc", ".ini"
        };

        public static string[] TextExtensions { get; } =
        {
            // Text based
            ".txt", ".md", ".markdown", ".rtf"
        };

        public static string[] DocumentExtensions { get; } =
        {
            // Document based
            ".doc", ".docx", ".html",
            ".odt", ".pdf", ".htm",

            // Sheet based
            ".xls", ".xlsx", ".ods",

            // Presentation based
            ".ppt", ".pptx"
        };

        public static string[] ImageExtensions { get; } =
        {
            // Special types
            ".apng", ".avif", ".gif",

            // JPEG types
            ".jpg", ".jpeg", ".jfif",
            ".pjpeg", ".pjp",

            // Other types
            ".png", ".svg", ".webp",
            ".bmp", ".tif", ".tiff"
        };

        public static string[] MediaExtensions { get; } =
        {
            ".mp4", ".mov", ".avi",
            ".wmv", ".flv", ".webm",
            ".mkv", ".avi"
        };

        public static string[] AudioExtensions { get; } =
        {
            ".3gp", ".flac", ".mp3",
            ".ogg", ".wav"
        };
    }
}