using OwlCore.Storage;

namespace SecureFolderFS.Tests.Helpers
{
    internal static partial class MockVaultHelpers
    {
        // Mock recovery key
        public const string V2_RECOVERY_KEY = "zepBGsfJZi/abgiEgca9Gtx4t0or5OH2J9PSg9KIQrU=@@@XfLMZXuHtQOTSvl9sVU4tOgZgvdjwWzTQCqZbppDE7c=";

        private const string V2_KEYSTORE_STRING = """
                                                  {
                                                    "c_encryptionKey": "CwoIRzFKP6Djc8a6Kf22bI0wDT+Ei6aO6eylyvLu3+G0IwRkmA+DhQ==",
                                                    "c_macKey": "lqYV/vPLd63xp9ngULhshlLs3B4DYl0DWdJnV7Ap3prMS4din89beQ==",
                                                    "salt": "Wsngg8yAWHMEJuOUsdk8Ow=="
                                                  }
                                                  """;

        private const string V2_SFCONFIG_STRING = """
                                                  {
                                                    "contentCipherScheme": "XChaCha20-Poly1305",
                                                    "filenameCipherScheme": "AES-SIV",
                                                    "authMode": "password",
                                                    "vaultId": "f89761bf-1ff3-487e-9212-943fb20c3ff5",
                                                    "hmacsha256mac": "fzYCLHtGxHrjsdwvjEarJaYVQlN2vR+Yqw5l4RW2+Ew=",
                                                    "version": 2
                                                  }
                                                  """;

        public static async Task<(IFolder, string)> CreateVaultV2Async(CancellationToken cancellationToken = default)
        {
            return (await SetupMockVault(V2_SFCONFIG_STRING, V2_KEYSTORE_STRING, cancellationToken), V2_RECOVERY_KEY);
        }
    }
}
