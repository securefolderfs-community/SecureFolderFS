using System;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract;

namespace SecureFolderFS.Core.FileSystem.Helpers.Paths.Native
{
    public static partial class NativePathHelpers
    {
        public static string EncryptName(string plaintextName, string plaintextParentFolder,
            FileSystemSpecifics specifics)
        {
            if (specifics.Security.NameCrypt is null)
                return plaintextName;

            var directoryId = AbstractPathHelpers.AllocateDirectoryId(specifics.Security, plaintextName);
            return EncryptName(plaintextName, plaintextParentFolder, specifics, directoryId);
        }

        public static string EncryptName(string plaintextName, string plaintextParentFolder,
            FileSystemSpecifics specifics, Span<byte> expendableDirectoryId)
        {
            if (specifics.Security.NameCrypt is null)
                return plaintextName;

            var result = GetDirectoryId(plaintextParentFolder, specifics, expendableDirectoryId);
            return specifics.Security.NameCrypt.EncryptName(plaintextName, result ? expendableDirectoryId : ReadOnlySpan<byte>.Empty) + Constants.Names.ENCRYPTED_FILE_EXTENSION;
        }

        public static string? DecryptName(string ciphertextName, string ciphertextParentFolder,
            FileSystemSpecifics specifics)
        {
            if (specifics.Security.NameCrypt is null)
                return ciphertextName;

            var directoryId = AbstractPathHelpers.AllocateDirectoryId(specifics.Security, ciphertextName);
            return DecryptName(ciphertextName, ciphertextParentFolder, specifics, directoryId);
        }

        public static string? DecryptName(string ciphertextName, string ciphertextParentFolder,
            FileSystemSpecifics specifics, Span<byte> expendableDirectoryId)
        {
            if (specifics.Security.NameCrypt is null)
                return ciphertextName;

            var result = GetDirectoryId(ciphertextParentFolder, specifics, expendableDirectoryId);
            var normalizedName = AbstractPathHelpers.NoCipherExtensionNormalizedName(ciphertextName);

            return specifics.Security.NameCrypt.DecryptName(normalizedName, result ? expendableDirectoryId : ReadOnlySpan<byte>.Empty);
        }
    }
}
