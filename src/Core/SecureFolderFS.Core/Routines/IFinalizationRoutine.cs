using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Core.Routines
{
    public interface IFinalizationRoutine : IAsyncInitialize, IDisposable
    {
        Task<IDisposable> FinalizeAsync(CancellationToken cancellationToken);
    }
}
