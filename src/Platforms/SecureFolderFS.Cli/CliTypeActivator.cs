using Microsoft.Extensions.DependencyInjection;

namespace SecureFolderFS.Cli;

internal static class CliTypeActivator
{
    public static object CreateInstance(IServiceProvider serviceProvider, Type type)
    {
        return ActivatorUtilities.CreateInstance(serviceProvider, type);
    }
}


