using System.Collections.Generic;
using FileInfo = Fsp.Interop.FileInfo;

namespace SecureFolderFS.Core.WinFsp.AppModels
{
    /// <summary>
    /// Holds the state of a single directory enumeration session.
    /// </summary>
    internal sealed class DirectoryEnumerationContext
    {
        public List<(string PlaintextName, FileInfo FileInfo)> Entries { get; }

        public int Index { get; set; }

        public DirectoryEnumerationContext(List<(string PlaintextName, FileInfo FileInfo)> entries)
        {
            Entries = entries;
        }
    }
}