using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Shared.Extensions
{
    public static class WrapperExtensions
    {
        public static IWrapper<T> GetDeepestWrapper<T>(this IWrapper<T> wrapper)
            where T : class
        {
            while (true)
            {
                if (wrapper.Inner is IWrapper<T> innerWrapper)
                {
                    wrapper = innerWrapper;
                    continue;
                }

                break;
            }

            return wrapper;
        }
    }
}
