using System.Collections.Generic;

namespace SecureFolderFS.Core.Routines
{
    // TODO: Needs docs
    public interface ICreationRoutine : ICredentialsRoutine
    {
        void SetOptions(IDictionary<string, string?> options);
    }
}
