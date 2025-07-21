using Android.Security.Keystore;
using Java.Security;

namespace SecureFolderFS.Maui.Platforms.Android.Helpers
{
    internal static class AndroidBiometricHelpers
    {
        public static KeyPairGenerator? GetKeyPairGenerator(string keyName, string keyStoreProvider)
        {
            var keyPairGenerator = KeyPairGenerator.GetInstance(KeyProperties.KeyAlgorithmRsa, keyStoreProvider);
            if (keyPairGenerator is null)
                return null;

            var builder = new KeyGenParameterSpec.Builder(
                    keyName,
                    KeyStorePurpose.Sign)
                .SetDigests(KeyProperties.DigestSha256)
                .SetSignaturePaddings(KeyProperties.SignaturePaddingRsaPkcs1)
                .SetUserAuthenticationRequired(true)
                .SetInvalidatedByBiometricEnrollment(false);

            keyPairGenerator.Initialize(builder.Build());
            return keyPairGenerator;
        }

        public static byte[]? SignData(Signature signature, byte[] data)
        {
            signature.Update(data);
            return signature.Sign();
        }
    }
}
