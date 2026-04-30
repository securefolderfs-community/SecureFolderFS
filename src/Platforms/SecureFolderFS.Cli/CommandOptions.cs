using CliFx.Binding;

namespace SecureFolderFS.Cli;

public abstract partial class VaultAuthOptions : CliGlobalOptions
{
    [CommandOption("password", Description = "Prompt for password interactively (masked input).")]
    public bool Password { get; set; }

    [CommandOption("password-stdin", Description = "Read password from stdin.")]
    public bool PasswordStdin { get; set; }

    [CommandOption("keyfile", Description = "Use an existing keyfile path.")]
    public string? KeyFile { get; set; }

    [CommandOption("twofa-password", Description = "Prompt for second-factor password interactively.")]
    public bool TwoFactorPassword { get; set; }

    [CommandOption("twofa-password-stdin", Description = "Read second-factor password from stdin.")]
    public bool TwoFactorPasswordStdin { get; set; }

    [CommandOption("twofa-keyfile", Description = "Use an existing second-factor keyfile path.")]
    public string? TwoFactorKeyFile { get; set; }

    [CommandOption("recovery-key", Description = "Unlock via recovery key instead of credentials.")]
    public string? RecoveryKey { get; set; }
}

public abstract class CreateAuthOptions : CliGlobalOptions
{
    [CommandOption("password", Description = "Prompt for password interactively (masked input).")]
    public bool Password { get; set; }

    [CommandOption("password-stdin", Description = "Read password from stdin.")]
    public bool PasswordStdin { get; set; }

    [CommandOption("keyfile", Description = "Use an existing keyfile path.")]
    public string? KeyFile { get; set; }

    [CommandOption("keyfile-generate", Description = "Generate a new keyfile and write it to this path.")]
    public string? KeyFileGenerate { get; set; }

    [CommandOption("twofa-password", Description = "Prompt for second-factor password interactively.")]
    public bool TwoFactorPassword { get; set; }

    [CommandOption("twofa-password-stdin", Description = "Read second-factor password from stdin.")]
    public bool TwoFactorPasswordStdin { get; set; }

    [CommandOption("twofa-keyfile", Description = "Use an existing second-factor keyfile path.")]
    public string? TwoFactorKeyFile { get; set; }

    [CommandOption("twofa-keyfile-generate", Description = "Generate a second-factor keyfile and write it to this path.")]
    public string? TwoFactorKeyFileGenerate { get; set; }
}

