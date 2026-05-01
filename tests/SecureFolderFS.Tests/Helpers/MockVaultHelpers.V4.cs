using OwlCore.Storage;
using SecureFolderFS.Tests.Models;

namespace SecureFolderFS.Tests.Helpers
{
    internal static partial class MockVaultHelpers
    {
        // Mock recovery key
        public const string V4_RECOVERY_KEY = "osNie57du4qOxSkuxcyYd36RsQI3xPVnUpPad/aych4=@@@7wmweOio0sGGljnvBiggxo/65ZlvTnTfVNFmwFZ7W8g=";

        private const string V4_KEYSTORE_STRING = """
                                                  {
                                                    "c_encryptionKey": "d0qVlgnquQr1NID0IdtrTd4pqzHkGxXWhHSpkrPuYtDXjVbm3ODpwQ==",
                                                    "c_macKey": "ZKhu3nbUjoqV6ZGtU/gitauQBuC76iCwuDnLD6oa3Pav1srYcQN/zw==",
                                                    "salt": "OEpZjy18/dbbS+i2LlmKjA==",
                                                    "c_softwareEntropy": "T9dHlJg4WLmuKM4Qz1rcdIn0QsCdiGNNtU6EyzN03OA=",
                                                    "entropyNonce": "buZqCJGGsukm87mP",
                                                    "entropyTag": "AvsZawlWTMG3gBpuavmu4g=="
                                                  }
                                                  """;

        private const string V4_SFCONFIG_STRING = """
                                                  {
                                                    "contentCipherScheme": "XChaCha20-Poly1305",
                                                    "filenameCipherScheme": "AES-SIV",
                                                    "filenameEncoding": "Base4K",
                                                    "recycleBinSize": -1,
                                                    "authMode": "password",
                                                    "vaultId": "8a1fbf8a-b986-4f8a-a3d7-59df8d203e3a",
                                                    "hmacsha256mac": "/gLIyGTCd8Y/bvj3YxY7JmmFEbCX3iDGYFVmeNMNw8w=",
                                                    "version": 4
                                                  }
                                                  """;

        public static async Task<(IFolder, string)> CreateVaultV4Async(MockVaultOptions? options,
            CancellationToken cancellationToken = default)
        {
            return (await SetupMockVault(V4_SFCONFIG_STRING, V4_KEYSTORE_STRING, options, cancellationToken), V4_RECOVERY_KEY);
        }
    }
}
