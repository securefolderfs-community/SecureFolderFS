using System;

namespace SecureFolderFS.Shared.Helpers
{
    public static class SafetyHelpers
    {
        public static T? NoThrowResult<T>(Func<T> action)
        {
            try
            {
                return action.Invoke();
            }
            catch (Exception ex)
            {
                _ = ex;
                return default;
            }
        }
    }
}
