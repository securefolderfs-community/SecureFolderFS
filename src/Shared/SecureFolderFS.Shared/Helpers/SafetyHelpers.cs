using System;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.Helpers
{
    public static class SafetyHelpers
    {
        public static void NoFailure(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                _ = ex;
            }
        }

        public static T? NoFailureResult<T>(Func<T> func)
        {
            try
            {
                return func.Invoke();
            }
            catch (Exception ex)
            {
                _ = ex;
                return default;
            }
        }

        public static async Task NoFailureAsync(Func<Task> func)
        {
            try
            {
                await func.Invoke();
            }
            catch (Exception ex)
            {
                _ = ex;
            }
        }

        public static async Task<T?> NoFailureAsync<T>(Func<Task<T>> func)
        {
            try
            {
                return await func.Invoke();
            }
            catch (Exception ex)
            {
                _ = ex;
                return default;
            }
        }
    }
}
