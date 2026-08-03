using System;

namespace SecureFolderFS.Core.FileSystem.Exceptions
{
    /// <summary>
    /// Exception thrown when a sidecar file (.sffsi) exists without a matching shortened file (.sffsn).
    /// </summary>
    public sealed class OrphanSidecarException : Exception
    {
        public OrphanSidecarException(string sidecarName)
            : base($"Orphan sidecar file has no matching shortened file: {sidecarName}")
        {
        }
    }
}
