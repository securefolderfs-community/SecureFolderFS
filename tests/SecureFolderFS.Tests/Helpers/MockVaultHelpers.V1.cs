using OwlCore.Storage;

namespace SecureFolderFS.Tests.Helpers
{
    internal static partial class MockVaultHelpers
    {
        private const string V1_KEYSTORE_STRING = """
                                               {
                                                 "c_encryptionKey": "xivDZLb7mlEOPXLP4Hn4LuJpUhryxHPvIQweUb/Nkd8gZoUP8Zwq1Q==",
                                                 "c_macKey": "Q2ckLYV6nnvmiXMEZjsZqGwwzcFUH8X1ntedH3YzVE7+/VGOc1TBVQ==",
                                                 "salt": "0D69Wmj9tqr3aSRiXKWpeA=="
                                               }
                                               """;

        private const string V1_SFCONFIG_STRING = """
                                               {
                                                 "contentCipherScheme": 4,
                                                 "filenameCipherScheme": 2,
                                                 "version": 1,
                                                 "hmacsha256mac": "Mr+ZRIcEi3I30L0kjXsvDvFwcuN8RnhpeXzOF+3htz4="
                                               }
                                               """;

        public static async Task<IFolder> CreateVaultV1Async(CancellationToken cancellationToken = default)
        {
            return await SetupMockVault(V1_SFCONFIG_STRING, V1_KEYSTORE_STRING, cancellationToken);
        }
    }
}