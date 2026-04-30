namespace SecureFolderFS.Cli;

internal static class CliExitCodes
{
    public const int Success = 0;
    public const int GeneralError = 1;
    public const int BadArguments = 2;
    public const int AuthenticationFailure = 3;
    public const int VaultUnreadable = 4;
    public const int MountFailure = 5;
    public const int MountStateError = 6;
}

