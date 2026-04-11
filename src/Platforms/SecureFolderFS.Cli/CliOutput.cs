using System.Text.RegularExpressions;
using CliFx.Infrastructure;

namespace SecureFolderFS.Cli;

internal static partial class CliOutput
{
    private const string Reset = "\u001b[0m";
    private const string Red = "\u001b[31m";
    private const string Yellow = "\u001b[33m";
    private const string Green = "\u001b[32m";

    public static void Info(IConsole console, CliGlobalOptions options, string message)
    {
        if (options.Quiet)
            return;

        console.Output.WriteLine(StripIfNeeded(options, message));
    }

    public static void Success(IConsole console, CliGlobalOptions options, string message)
    {
        if (options.Quiet)
            return;

        var line = options.NoColor ? message : $"{Green}{message}{Reset}";
        console.Output.WriteLine(StripIfNeeded(options, line));
    }

    public static void Warning(IConsole console, CliGlobalOptions options, string message)
    {
        if (options.Quiet)
            return;

        var line = options.NoColor ? $"warning: {message}" : $"{Yellow}warning:{Reset} {message}";
        console.Output.WriteLine(StripIfNeeded(options, line));
    }

    public static void Error(IConsole console, CliGlobalOptions options, string message)
    {
        var line = options.NoColor ? $"error: {message}" : $"{Red}error:{Reset} {message}";
        console.Error.WriteLine(StripIfNeeded(options, line));
    }

    public static string StripIfNeeded(CliGlobalOptions options, string value)
    {
        return options.NoColor ? AnsiRegex().Replace(value, string.Empty) : value;
    }

    [GeneratedRegex("\\x1B\\[[0-9;]*[A-Za-z]")]
    private static partial Regex AnsiRegex();
}

