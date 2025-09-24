using OwlCore.Storage;

namespace SecureFolderFS.Tests.Helpers
{
    internal static partial class MockVaultHelpers
    {
        // Mock recovery key
        public const string V3_RECOVERY_KEY = "+pzioG4vpqMrAwNI5MKlWuTKyYC8HLIuTWYqSQjXp/w=@@@cWKjobcRm3IYsplp67kT9a+eE4vMj0uAnrWQqOginDY=";

        private const string V3_KEYSTORE_STRING = """
                                                  {
                                                    "c_encryptionKey": "JPZbn8xozERWUErKhEAZo+MONqEk5rS4URKkN9iwGR3gkyaupg+icg==",
                                                    "c_macKey": "hvtHrkZiyXkGoA1sqedcMY2VVoGaVi5rd2z2XZKOoHkEbuB8FsM1dA==",
                                                    "salt": "sxiVu25C0a44t/u1Dc87HA=="
                                                  }
                                                  """;

        private const string V3_SFCONFIG_STRING = """
                                                  {
                                                    "contentCipherScheme": "XChaCha20-Poly1305",
                                                    "filenameCipherScheme": "AES-SIV",
                                                    "filenameEncoding": "Base4K",
                                                    "recycleBinSize": 0,
                                                    "authMode": "password",
                                                    "vaultId": "3a169788-6149-4583-ad92-f68113e70e23",
                                                    "hmacsha256mac": "gTTCKQg10Aote0gXWOeVzHXw4VPtYtSARHAN01XbyD0=",
                                                    "version": 3
                                                  }
                                                  """;

        public static async Task<(IFolder, string)> CreateVaultV3Async(MockVaultOptions? options, CancellationToken cancellationToken = default)
        {
            return (await SetupMockVault(V3_SFCONFIG_STRING, V3_KEYSTORE_STRING, options, cancellationToken), V3_RECOVERY_KEY);
        }
    }
}
