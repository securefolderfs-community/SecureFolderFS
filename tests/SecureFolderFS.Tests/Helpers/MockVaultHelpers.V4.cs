using OwlCore.Storage;
using SecureFolderFS.Tests.Models;

namespace SecureFolderFS.Tests.Helpers
{
    internal static partial class MockVaultHelpers
    {
        // Mock recovery key
        public const string V4_RECOVERY_KEY = "xkniDwR94w55X9x4hf7xktz0IwGGal/2JKDMn1w2r9c=@@@KgiXI8E9dZo9K8LshLAnEDbGV8EQs3TQn6MRUGfOmVQ=";

        private const string V4_KEYSTORE_STRING = """
                                                  {
                                                    "c_encryptionKey": "GK/Gc0UphVHpb/utm61odQJGH6j7IIc9RAGWQTTrcugxTOXwOxkwhA==",
                                                    "c_macKey": "A7jTnbT9RHfZ466cU/DLHIp7164L4iXqnWTpGOT5ast9pgnrccAi7A==",
                                                    "salt": "aSNkObtR5gXuR5uNkeSygw==",
                                                    "c_softwareEntropy": "SRM8vmYDfy0F8EWGzN1/5YAuMRxvGsXH6eiQhwJJ6/s=",
                                                    "entropyNonce": "tsuXKhJA6SvzlB8+",
                                                    "entropyTag": "2dD9acgmpY9/xQoW6I6kmA=="
                                                  }
                                                  """;

        private const string V4_SFCONFIG_STRING = """
                                                  {
                                                    "contentCipherScheme": "AES-GCM",
                                                    "filenameCipherScheme": "AES-SIV",
                                                    "filenameEncoding": "Base4K",
                                                    "filenameShortening": 0,
                                                    "recycleBinSize": 0,
                                                    "authMode": "password",
                                                    "vaultId": "2ecbdd1f-3cd3-4b4f-88d9-d54da869aa3c",
                                                    "appPlatform": null,
                                                    "complementGeneration": 0,
                                                    "hmacsha256mac": "ZkYPf3EMwVhS7bjGasakr3CrN54bMRmlZUBvg7hZycw=",
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
