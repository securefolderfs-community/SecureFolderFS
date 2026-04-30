using CliFx.Attributes;
using CliFx.Infrastructure;

namespace SecureFolderFS.Cli;

public abstract class CliGlobalOptions : CliFx.ICommand
{
    [CommandOption("quiet", 'q', Description = "Suppress decorative/info output. Errors always go to stderr.")]
    public bool Quiet { get; init; }

    [CommandOption("no-color", Description = "Disable ANSI color output.")]
    public bool NoColor { get; init; }

    public abstract ValueTask ExecuteAsync(IConsole console);
}


