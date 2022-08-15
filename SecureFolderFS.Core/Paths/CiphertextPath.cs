using SecureFolderFS.Core.Sdk.Paths;

namespace SecureFolderFS.Core.Paths
{
    internal sealed class CiphertextPath : BasePath, ICiphertextPath
    {
        public override string Path { get; protected init; }

        public CiphertextPath(string rawPath)
        {
            Path = rawPath;
        }
    }
}
