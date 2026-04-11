namespace SecureFolderFS.Tests.CliTests;

public sealed record CliExecutionResult(
    int AppExitCode,
    int ProcessExitCode,
    string StandardOutput,
    string StandardError);


