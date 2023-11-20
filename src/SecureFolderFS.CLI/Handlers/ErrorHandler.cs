using CommandLine;
using SecureFolderFS.CLI.Utilities;

namespace SecureFolderFS.CLI.Handlers
{
    public sealed class ErrorHandler : SingletonBase<ErrorHandler>, IHandler<IEnumerable<Error>>
    {
        public Task HandleAsync(IEnumerable<Error> errors, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}