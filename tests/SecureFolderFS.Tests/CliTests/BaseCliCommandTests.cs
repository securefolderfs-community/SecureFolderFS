using NUnit.Framework;

namespace SecureFolderFS.Tests.CliTests;

public abstract class BaseCliCommandTests
{
    private readonly List<string> _tempDirectories = [];

    [SetUp]
    public void ResetExitCode()
    {
        Environment.ExitCode = 0;
    }

    [TearDown]
    public void CleanupTempDirectories()
    {
        foreach (var directory in _tempDirectories)
        {
            try
            {
                if (Directory.Exists(directory))
                    Directory.Delete(directory, recursive: true);
            }
            catch
            {
                // Best-effort cleanup only.
            }
        }

        _tempDirectories.Clear();
    }

    protected Task<CliExecutionResult> RunCliAsync(params string[] args)
    {
        return CliTestHost.RunAsync(args);
    }

    protected Task<CliExecutionResult> RunCliAsync(string[] args, IReadOnlyDictionary<string, string?> environmentVariables)
    {
        return CliTestHost.RunAsync(args, environmentVariables: environmentVariables);
    }

    protected string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"sffs-cli-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        _tempDirectories.Add(path);

        return path;
    }
}

