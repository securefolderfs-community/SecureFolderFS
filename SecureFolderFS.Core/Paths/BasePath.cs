using SecureFolderFS.Core.Sdk.Paths;

namespace SecureFolderFS.Core.Paths
{
    internal abstract class BasePath : IPath
    {
        public virtual string Path { get; protected init; }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is IPath path)
            {
                return Path.Equals(path.Path);
            }

            return base.Equals(obj);
        }
    }
}
