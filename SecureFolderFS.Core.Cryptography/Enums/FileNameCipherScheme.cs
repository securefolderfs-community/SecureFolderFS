namespace SecureFolderFS.Core.Cryptography.Enums
{
    public enum FileNameCipherScheme : uint
    {
        /// <summary>
        /// Not defined, SecureFolderFS won't function with this flag.
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// File names stay unencrypted.
        /// </summary>
        None = 1,
        
        /// <summary>
        /// AES-SIV file name encryption.
        /// </summary>
        AES_SIV = 2
    }
}
