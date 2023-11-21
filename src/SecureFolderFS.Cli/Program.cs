using CommandLine;
using CommandLine.Text;
using SecureFolderFS.Cli.Handlers;
using SecureFolderFS.Cli.Options;

namespace SecureFolderFS.Cli
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            var parser = new Parser(with =>
            {
                with.HelpWriter = null;
                with.CaseInsensitiveEnumValues = true;
            });
            var parserResult = await parser.ParseArguments<CreateVaultOptions, UnlockVaultOptions>(args)
                .WithParsedAsync<CreateVaultOptions>(async options => await CreateVaultHandler.Instance.HandleAsync(options));
            parserResult.WithNotParsed(_ => DisplayHelp(parserResult));
        }
        
        private static void DisplayHelp<T>(ParserResult<T> result)
        {  
            var helpText = HelpText.AutoBuild(result, h =>
            {
                h.AddEnumValuesToHelpText = true;
                return HelpText.DefaultParsingErrorsHandler(result, h);
            }, e => e);
            Console.WriteLine(helpText);
        }
    }
}