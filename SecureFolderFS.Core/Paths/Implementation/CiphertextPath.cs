namespace SecureFolderFS.Core.Paths.Implementation
{
    internal sealed class CiphertextPath : BasePath, ICiphertextPath
    {
        public override string Path { get; protected init; }

        public CiphertextPath(string rawPath)
        {
            this.Path = rawPath;
        }
    }
}
