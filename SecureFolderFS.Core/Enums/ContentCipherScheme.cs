namespace SecureFolderFS.Core.Enums
{
    public enum ContentCipherScheme : uint
    {
        /// <summary>
        /// Not defined, SecureFolderFS won't function with this flag.
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// AES-CTR + HMAC-SHA256 file content encryption and authentication.
        /// </summary>
        AES_CTR_HMAC = 1,

        /// <summary>
        /// AES-GCM file content encryption and authentication.
        /// </summary>
        AES_GCM = 2,

        /// <summary>
        /// XChaCha20-Poly1305 file content encryption and authentication.
        /// </summary>
        XChaCha20_Poly1305 = 4
    }
}
