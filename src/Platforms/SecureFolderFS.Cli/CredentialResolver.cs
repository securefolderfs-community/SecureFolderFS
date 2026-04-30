using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using static SecureFolderFS.Core.Constants.Vault.Authentication;

namespace SecureFolderFS.Cli;

internal sealed class ResolvedAuthentication : IDisposable
{
    public required IKeyUsage Passkey { get; init; }
    public required string[] Methods { get; init; }

    public void Dispose()
    {
        Passkey.Dispose();
    }
}

internal static class CredentialResolver
{
    public static async Task<ResolvedAuthentication?> ResolveCreateAuthenticationAsync(CreateAuthOptions options, CredentialReader reader, string vaultId)
    {
        var primary = await ResolveFactorAsync(
            options.Password,
            options.PasswordStdin,
            options.KeyFile,
            options.KeyFileGenerate,
            "SFFS_PASSWORD",
            "SFFS_KEYFILE",
            "Primary password: ",
            reader,
            vaultId);

        if (primary is null)
            return null;

        var secondary = await ResolveFactorAsync(
            options.TwoFactorPassword,
            options.TwoFactorPasswordStdin,
            options.TwoFactorKeyFile,
            options.TwoFactorKeyFileGenerate,
            null,
            null,
            "Second-factor password: ",
            reader,
            vaultId);

        return Build(primary.Value, secondary);
    }

    public static async Task<ResolvedAuthentication?> ResolveAuthenticationAsync(VaultAuthOptions options, CredentialReader reader)
    {
        var primary = await ResolveFactorAsync(
            options.Password,
            options.PasswordStdin,
            options.KeyFile,
            null,
            "SFFS_PASSWORD",
            "SFFS_KEYFILE",
            "Password: ",
            reader,
            null);

        if (primary is null)
            return null;

        var secondary = await ResolveFactorAsync(
            options.TwoFactorPassword,
            options.TwoFactorPasswordStdin,
            options.TwoFactorKeyFile,
            null,
            null,
            null,
            "Second-factor password: ",
            reader,
            null);

        return Build(primary.Value, secondary);
    }

    private static ResolvedAuthentication Build((string method, IKeyUsage key) primary, (string method, IKeyUsage key)? secondary)
    {
        if (secondary is null)
        {
            return new ResolvedAuthentication
            {
                Passkey = primary.key,
                Methods = [primary.method]
            };
        }

        var keySequence = new KeySequence();
        keySequence.Add(primary.key);
        keySequence.Add(secondary.Value.key);

        return new ResolvedAuthentication
        {
            Passkey = keySequence,
            Methods = [primary.method, secondary.Value.method]
        };
    }

    private static async Task<(string method, IKeyUsage key)?> ResolveFactorAsync(
        bool usePromptPassword,
        bool useStdinPassword,
        string? keyFile,
        string? generateKeyFile,
        string? passwordEnvironmentName,
        string? keyFileEnvironmentName,
        string prompt,
        CredentialReader reader,
        string? vaultId)
    {
        var envPassword = passwordEnvironmentName is null ? null : Environment.GetEnvironmentVariable(passwordEnvironmentName);
        var password = await reader.ReadPasswordAsync(usePromptPassword, useStdinPassword, prompt, envPassword);
        if (!string.IsNullOrEmpty(password))
            return (AUTH_PASSWORD, new DisposablePassword(password));

        if (!string.IsNullOrWhiteSpace(generateKeyFile))
        {
            if (string.IsNullOrWhiteSpace(vaultId))
            {
                // TODO: verify - keyfile generation for login/update flows may need explicit vault id derivation from config.
                throw new InvalidOperationException("Keyfile generation requires a known vault identifier.");
            }

            return (AUTH_KEYFILE, await reader.GenerateKeyFileAsync(generateKeyFile, vaultId));
        }

        var envKeyFile = keyFileEnvironmentName is null ? null : Environment.GetEnvironmentVariable(keyFileEnvironmentName);
        var keyFilePath = reader.ReadKeyFilePath(keyFile, envKeyFile);
        if (!string.IsNullOrWhiteSpace(keyFilePath))
            return (AUTH_KEYFILE, await reader.ReadKeyFileAsKeyAsync(keyFilePath));

        return null;
    }
}

