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

        public static IWrapper<T> GetWrapperAt<T>(this IWrapper<T> wrapper, int level)
        {
            while (true)
            {
                if (wrapper.Inner is IWrapper<T> innerWrapper)
                {
                    wrapper = innerWrapper;
                    level--;
                    continue;
                }

                if (level == 0)
                    return wrapper;
            }
        }

        public static IWrapper<T> GetWrapperAt<T>(this IWrapper<T> wrapper, string typeName)
        {
            while (true)
            {
                if (wrapper.GetType().Name == typeName)
                    return wrapper;

                if (wrapper.Inner is IWrapper<T> innerWrapper)
                {
                    wrapper = innerWrapper;
                    continue;
                }

                break;
            }

            throw new InvalidOperationException($"Could not find wrapper at {typeName}.");
        }

        public static IWrapper<T> GetWrapperBelow<T, TAbove>(this IWrapper<T> wrapper)
        {
            var aboveWrapper = GetWrapperAt<T, TAbove>(wrapper);
            if (aboveWrapper is { Inner: IWrapper<T> belowWrapper })
                return belowWrapper;

            throw new InvalidOperationException($"Could not find wrapper below level {typeof(TAbove).Name}.");
        }

        public static IWrapper<T> AsWrapper<T>(this object presumedWrapper)
        {
            if (presumedWrapper is not IWrapper<T> wrapper)
                throw new ArgumentException($"The {nameof(presumedWrapper)} is not of type {typeof(IWrapper<T>)}.");

            return wrapper;
        }
    }
}
