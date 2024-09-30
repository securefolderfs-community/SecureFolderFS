using System.Collections.Generic;

namespace SecureFolderFS.Core.Routines
{
    public interface IOptionsRoutine
    {
        void SetOptions(IDictionary<string, object?> options);
    }
}
