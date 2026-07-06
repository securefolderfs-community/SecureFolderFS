using CliFx;
using CliFx.Binding;
using CliFx.Infrastructure;

namespace SecureFolderFS.Cli;

public abstract class CliGlobalOptions : ICommand
{
    [CommandOption("quiet", 'q', Description = "Suppress decorative/info output. Errors always go to stderr.")]
    public bool Quiet { get; set; }

    [CommandOption("no-color", Description = "Disable ANSI color output.")]
    public bool NoColor { get; set; }

    public abstract ValueTask ExecuteAsync(IConsole console);
}
