using CommandLine;
using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.Cli.Handlers
{
    public sealed class ErrorHandler : SingletonBase<ErrorHandler>, IHandler<IEnumerable<Error>>
    {
        public Task HandleAsync(IEnumerable<Error> errors, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}