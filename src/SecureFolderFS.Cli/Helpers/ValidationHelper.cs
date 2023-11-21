using System.Reflection;
using CommandLine;
using SecureFolderFS.Cli.Attributes;

namespace SecureFolderFS.Cli.Helpers
{
    public static class ValidationHelper
    {
        public static void ValidateOptions<TOptions>(TOptions options)
        {
            foreach (var property in typeof(TOptions).GetProperties())
            {
                var validOptions = property.GetCustomAttribute<ValidOptionsAttribute>();
                if (validOptions is null)
                    continue;

                var option = property.GetCustomAttribute<OptionAttribute>();
                if (option is null)
                    continue;
                
                

                if (!validOptions.Values.Select(x => x.ToLower()).Contains(((string)property.GetValue(options)).ToLower()))
                {
                    Console.Error.WriteLine($"The parameter \"{option.LongName}\" has an invalid value. Valid values are (case insensitive): {string.Join(", ", validOptions.Values)}.");
                    Environment.Exit(-1);
                }
            }
        }
    }
}