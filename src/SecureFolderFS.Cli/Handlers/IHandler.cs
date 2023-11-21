namespace SecureFolderFS.Cli.Handlers
{
    public interface IHandler<TOptions>
    {
        public Task HandleAsync(TOptions options, CancellationToken cancellationToken = default);
    }
}