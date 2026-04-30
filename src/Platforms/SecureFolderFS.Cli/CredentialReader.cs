using System.Security.Cryptography;
using System.Text;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Cli;

public sealed class CredentialReader
{
    private const int KeyFileRandomLength = 128;

    public async Task<string?> ReadPasswordAsync(bool prompt, bool readFromStdin, string promptText, string? environmentVariable)
    {
        if (readFromStdin)
        {
            var stdinValue = await Console.In.ReadLineAsync();
            return string.IsNullOrEmpty(stdinValue) ? null : stdinValue;
        }

        if (prompt)
            return ReadMaskedPassword(promptText);

        return string.IsNullOrWhiteSpace(environmentVariable) ? null : environmentVariable;
    }

    public string? ReadRecoveryKey(string? explicitValue, string? environmentValue)
    {
        return !string.IsNullOrWhiteSpace(explicitValue)
            ? explicitValue
            : string.IsNullOrWhiteSpace(environmentValue) ? null : environmentValue;
    }

    public string? ReadKeyFilePath(string? explicitValue, string? environmentValue)
    {
        return !string.IsNullOrWhiteSpace(explicitValue)
            ? explicitValue
            : string.IsNullOrWhiteSpace(environmentValue) ? null : environmentValue;
    }

    public async Task<IKeyUsage> ReadKeyFileAsKeyAsync(string path)
    {
        var expandedPath = Path.GetFullPath(path);
        var keyBytes = await File.ReadAllBytesAsync(expandedPath);
        if (keyBytes.Length == 0)
            throw new InvalidDataException("The key file is empty.");

        return ManagedKey.TakeOwnership(keyBytes);
    }

    public async Task<IKeyUsage> GenerateKeyFileAsync(string outputPath, string vaultId)
    {
        var fullPath = Path.GetFullPath(outputPath);
        var parent = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(parent))
            Directory.CreateDirectory(parent);

        var idBytes = Encoding.ASCII.GetBytes(vaultId);
        var keyBytes = new byte[KeyFileRandomLength + idBytes.Length];

        RandomNumberGenerator.Fill(keyBytes.AsSpan(0, KeyFileRandomLength));
        idBytes.CopyTo(keyBytes.AsSpan(KeyFileRandomLength));

        await File.WriteAllBytesAsync(fullPath, keyBytes);
        return ManagedKey.TakeOwnership(keyBytes);
    }

    private static string? ReadMaskedPassword(string prompt)
    {
        Console.Write(prompt);
        var buffer = new StringBuilder();

        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                break;
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (buffer.Length <= 0)
                    continue;

                buffer.Length -= 1;
                Console.Write("\b \b");
                continue;
            }

            if (char.IsControl(key.KeyChar))
                continue;

            buffer.Append(key.KeyChar);
            Console.Write('*');
        }

        return buffer.Length == 0 ? null : buffer.ToString();
    }
}


