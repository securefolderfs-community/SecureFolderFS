using SecureFolderFS.Core.Sdk.Paths;

namespace SecureFolderFS.Core.Paths
{
    internal sealed class CleartextPath : BasePath, ICleartextPath
    {
        public override string Path { get; protected init; }

        public CleartextPath(string rawPath)
        {
            Path = rawPath;
        }
    }
}
