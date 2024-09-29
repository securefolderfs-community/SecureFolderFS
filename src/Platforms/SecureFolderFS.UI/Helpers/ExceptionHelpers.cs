using System;
using System.Diagnostics;
using System.IO;

namespace SecureFolderFS.UI.Helpers
{
    public static class ExceptionHelpers
    {
        public static string? FormatException(Exception? ex)
        {
            if (ex is null)
                return null;

            var exceptionString = string.Empty;

            exceptionString += DateTime.Now.ToString(Constants.Application.EXCEPTION_BLOCK_DATE_FORMAT);
            exceptionString += "\n";
            exceptionString += $">>> HRESULT {ex.HResult}\n";
            exceptionString += $">>> MESSAGE {ex.Message}\n";
            exceptionString += $">>> STACKTRACE {ex.StackTrace}\n";
            exceptionString += $">>> INNER {ex.InnerException}\n";
            exceptionString += $">>> SOURCE {ex.Source}\n\n";

            return exceptionString;
        }

        public static void TryWriteToFile(string appDirectory, Exception? ex)
        {
            var formattedException = FormatException(ex);
            if (formattedException is null)
                return;

            try
            {
                var filePath = Path.Combine(appDirectory, Constants.Application.EXCEPTION_LOG_FILENAME);
                File.AppendAllText(filePath, $"\n\n\t\tEXCEPTION EVENT: {DateTime.Now}\n{formattedException}");
            }
            catch (Exception ex2)
            {
                Debug.WriteLine(ex2);
                Debugger.Break();
            }
        }
    }
}
