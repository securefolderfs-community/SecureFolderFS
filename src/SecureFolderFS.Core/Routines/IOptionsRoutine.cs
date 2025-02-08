using System.Collections.Generic;

namespace SecureFolderFS.Core.Routines
{
    public interface IOptionsRoutine : IFinalizationRoutine
    {
        void SetOptions(IDictionary<string, object?> options);
    }
}
