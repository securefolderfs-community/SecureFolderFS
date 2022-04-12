using System;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.Extensions
{
    public static class AsyncExtensions
    {
        public static void RunAndForget(Action action)
        {
            try
            {
                _ = Task.Run(action).ConfigureAwait(false);
            }
            catch { }
        }
    }
}
