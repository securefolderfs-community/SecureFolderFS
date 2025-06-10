using SecureFolderFS.Shared.Enums;

namespace SecureFolderFS.Shared.Models
{
    public struct TypeClassification
    {
        /// <summary>
        /// Gets the MIME content type.
        /// </summary>
        public string MimeType { get; }

        /// <summary>
        /// Gets the hint which may or may not indicate the correct type.
        /// </summary>
        public TypeHint TypeHint { get; }

        /// <summary>
        /// Gets the content extension, if any.
        /// </summary>
        public string? Extension { get; }

        public TypeClassification(string mimeType, TypeHint typeHint, string? extension = null)
        {
            MimeType = mimeType;
            TypeHint = typeHint;
            Extension = extension;
        }
    }
}