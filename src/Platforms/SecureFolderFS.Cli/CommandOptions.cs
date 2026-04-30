using CliFx.Attributes;

namespace SecureFolderFS.Cli;

public abstract partial class VaultAuthOptions : CliGlobalOptions
{
    [CommandOption("password", Description = "Prompt for password interactively (masked input).")]
    public bool Password { get; init; }

    [CommandOption("password-stdin", Description = "Read password from stdin.")]
    public bool PasswordStdin { get; init; }

    [CommandOption("keyfile", Description = "Use an existing keyfile path.")]
    public string? KeyFile { get; init; }

    [CommandOption("twofa-password", Description = "Prompt for second-factor password interactively.")]
    public bool TwoFactorPassword { get; init; }

    [CommandOption("twofa-password-stdin", Description = "Read second-factor password from stdin.")]
    public bool TwoFactorPasswordStdin { get; init; }

    [CommandOption("twofa-keyfile", Description = "Use an existing second-factor keyfile path.")]
    public string? TwoFactorKeyFile { get; init; }

    [CommandOption("recovery-key", Description = "Unlock via recovery key instead of credentials.")]
    public string? RecoveryKey { get; init; }
}

public abstract class CreateAuthOptions : CliGlobalOptions
{
    [CommandOption("password", Description = "Prompt for password interactively (masked input).")]
    public bool Password { get; init; }

    [CommandOption("password-stdin", Description = "Read password from stdin.")]
    public bool PasswordStdin { get; init; }

    [CommandOption("keyfile", Description = "Use an existing keyfile path.")]
    public string? KeyFile { get; init; }

    [CommandOption("keyfile-generate", Description = "Generate a new keyfile and write it to this path.")]
    public string? KeyFileGenerate { get; init; }

    [CommandOption("twofa-password", Description = "Prompt for second-factor password interactively.")]
    public bool TwoFactorPassword { get; init; }

    [CommandOption("twofa-password-stdin", Description = "Read second-factor password from stdin.")]
    public bool TwoFactorPasswordStdin { get; init; }

    [CommandOption("twofa-keyfile", Description = "Use an existing second-factor keyfile path.")]
    public string? TwoFactorKeyFile { get; init; }

    [CommandOption("twofa-keyfile-generate", Description = "Generate a second-factor keyfile and write it to this path.")]
    public string? TwoFactorKeyFileGenerate { get; init; }
}

