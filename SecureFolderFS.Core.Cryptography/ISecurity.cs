using System;
using System.Security.Cryptography;

namespace SecureFolderFS.Core.Cryptography
{
    internal interface ISecurity
    {
        IFormatProvider NameCryptFormatProvider { get; }

        ICryptoTransform CreateEncryptor();

        ICryptoTransform CreateDecryptor();
    }
}
