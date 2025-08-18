using System.Collections;
using System.Diagnostics;

namespace SecureFolderFS.Shared.Helpers
{
    public static class DebuggingHelpers
    {
        private const bool DEBUG_LOGGING = true;

        public static void PrintEnumerable(IEnumerable enumerable)
        {
            if (!CanLog()) return;

            foreach (var item in enumerable)
            {
                Debug.WriteLine(item);
            }
        }

        public static void PrintEnumerableInline(IEnumerable enumerable)
        {
            if (!CanLog()) return;

            foreach (var item in enumerable)
            {
                Debug.Write($"{item} ");
            }
        }

        public static T OutputToDebugAndContinue<T>(this T target, string message)
        {
            if (!CanLog()) return target;

            Debug.WriteLine(message);

            return target;
        }

        public static void OutputToDebug(object obj)
        {
            OutputToDebug(obj.ToString() ?? "NULL-MESSAGEs");
        }

        public static void OutputToDebug(string message)
        {
            if (!CanLog()) return;

            Debug.WriteLine(message);
        }

        private static bool CanLog()
        {
#if !DEBUG
            return false;
#endif

            return DEBUG_LOGGING;
        }
    }
}
