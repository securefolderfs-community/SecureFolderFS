using System;
using System.Collections.Generic;

namespace SecureFolderFS.Sdk.DataModels
{
    /// <summary>
    /// Holds the configuration of the Journal widget of an individual vault.
    /// </summary>
    /// <param name="Folders">
    /// The folder entries are kept in, relative to the vault root and separated by '/'. The first one
    /// receives new entries unless another is chosen.
    /// </param>
    [Serializable]
    public sealed record JournalDataModel(IList<string> Folders)
    {
        public IList<string> Folders { get; set; } = Folders;
    }
}
