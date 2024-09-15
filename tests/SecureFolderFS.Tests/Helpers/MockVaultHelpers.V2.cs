using OwlCore.Storage;

namespace SecureFolderFS.Tests.Helpers
{
    internal static partial class MockVaultHelpers
    {
        // Mock recovery key
        public const string V2_RECOVERY_KEY = "tjrkk1TRcnSG7v8pIEFdhO3PKteBj1l3/xX/N4AkDpU=@@@uOVXKhcbK/Wshqx7WXsGKXJXbwiTZ2gu6UBSwHfu1JU=";

        private const string V2_KEYSTORE_STRING = """
                                                  {
                                                    "c_encryptionKey": "Gz2pwsoj7yQLDsxcEZVFk5YkQNUm8Ctpq2dUeRQvybVPDJh5bqxCXA==",
                                                    "c_macKey": "7bIu/eF5wCQ0as2MY/Gf+dJnEyLHq9yFBRd7Exl/cq3+eQsJsbFjEQ==",
                                                    "salt": "McqBPpjXQ+xbqBB0HtZ8Wg=="
                                                  }
                                                  """;

        private const string V2_SFCONFIG_STRING = """
                                                  {
                                                    "contentCipherScheme": "XChaCha20-Poly1305",
                                                    "filenameCipherScheme": "AES-SIV",
                                                    "authMode": "Password",
                                                    "vaultId": "16da6b20-49a5-4fd5-b0e8-de607db10d63",
                                                    "hmacsha256mac": "9v/eMaxelNay/aYUUuBsx0gb6GJugVJRXb+6Eq5b8Sc=",
                                                    "version": 2
                                                  }
                                                  """;

        public static async Task<IFolder> CreateVaultV2Async(CancellationToken cancellationToken = default)
        {
            return await SetupMockVault(V2_SFCONFIG_STRING, V2_KEYSTORE_STRING, cancellationToken);
        }
    }
}
