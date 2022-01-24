using System.Collections;
using System.Diagnostics;

namespace SecureFolderFS.Core.Helpers
{
    internal static class DebugHelpers
    {
        public static void PrintEnumerable(IEnumerable enumerable)
        {
#if !DEBUG
            return;
#endif

            foreach (var item in enumerable)
            {
                Debug.WriteLine(item);
            }
        }

        public static void PrintEnumerableInline(IEnumerable enumerable)
        {
#if !DEBUG
            return;
#endif

            foreach (var item in enumerable)
            {
                Debug.Write($"{item} ");
            }
        }

        public static T OutputToDebugAndContinue<T>(this T target, string message)
        {
#if DEBUG
            Debug.WriteLine(message);
#endif

            return target;
        }
    }
}
