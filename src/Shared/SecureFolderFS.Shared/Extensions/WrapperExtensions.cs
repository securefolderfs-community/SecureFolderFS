using System;
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

        public static IWrapper<T> GetWrapperAt<T, TAt>(this IWrapper<T> wrapper)
        {
            while (true)
            {
                if (wrapper is TAt)
                    return wrapper;

                if (wrapper.Inner is IWrapper<T> innerWrapper)
                {
                    wrapper = innerWrapper;
                    continue;
                }

                break;
            }

            throw new InvalidOperationException($"Could not find wrapper at level {typeof(TAt).Name}.");
        }

        public static IWrapper<T> GetWrapperBelow<T, TAbove>(this IWrapper<T> wrapper)
        {
            var aboveWrapper = GetWrapperAt<T, TAbove>(wrapper);
            if (aboveWrapper is { Inner: IWrapper<T> belowWrapper })
                return belowWrapper;

            throw new InvalidOperationException($"Could not find wrapper below level {typeof(TAbove).Name}.");
        }
    }
}
