namespace SecureFolderFS.Core.Paths.Implementation
{
    internal sealed class CleartextPath : BasePath, ICleartextPath
    {
        public override string Path { get; protected init; }

        public CleartextPath(string rawPath)
        {
            this.Path = rawPath;
        }
    }
}
