using OwlCore.Storage;
using SecureFolderFS.Tests.Models;

namespace SecureFolderFS.Tests.Helpers
{
    internal static partial class MockVaultHelpers
    {
        // Mock recovery key
        public const string V4_RECOVERY_KEY = "Ww0/Rx6XHEB87y4WWfw12xo7Xfyk67EB3FNUlWdaG/k=@@@ndVd2mEgV/9Sq9xhLKa4ZaACwQH+7JVzfb7rTLBZK2s=";

        private const string V4_KEYSTORE_STRING = """
                                                  {
                                                    "c_encryptionKey": "wALUX7wq5cZ45yB3HncCiiXZ3OiQmOTZj5MU/T03dxldD3pyh11C5g==",
                                                    "c_macKey": "1DrsQmRH4X6CgM08aRqeANYXxN6NWlNrzsbp6DKeXErrE+KfYRS08g==",
                                                    "salt": "e5nJCCu+uJZ3uAwio+iXOg==",
                                                    "c_softwareEntropy": "bYWy61+0lXUb8e9ZLmasECuCZWmUTaKC8BIJvEofix4=",
                                                    "entropyNonce": "jzd/bdatTE2uOoMa",
                                                    "entropyTag": "OwjR5RFBmvl7Aaf0rwQ8yw=="
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
                                                    "vaultId": "0b47eb66-1e58-451f-a72f-f1f5b34295b6",
                                                    "appPlatform": null,
                                                    "hmacsha256mac": "fpSCs1rfVwtWeCikRcSeJimORlfN3f+MCjed9nobofA=",
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
