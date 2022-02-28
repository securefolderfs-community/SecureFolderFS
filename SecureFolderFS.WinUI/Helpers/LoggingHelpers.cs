using System;
using System.Diagnostics;
using System.IO;
using Windows.Storage;

namespace SecureFolderFS.WinUI.Helpers
{
    internal static class LoggingHelpers
    {
        public static void SafeLogExceptionToFile(Exception ex)
        {
            if (ex == null)
            {
                return;
            }

            var exceptionString = string.Empty;

            exceptionString += DateTime.Now.ToString(Constants.Application.EXCEPTION_BLOCK_DATE_FORMAT);
            exceptionString += "\n";
            exceptionString += $">>> HRESULT {ex.HResult}\n";
            exceptionString += $">>> MESSAGE {ex.Message}\n";
            exceptionString += $">>> STACKTRACE {ex.StackTrace}\n";
            exceptionString += $">>> INNER {ex.InnerException}\n";
            exceptionString += $">>> SOURCE {ex.Source}\n\n";

            LogToFile(exceptionString);
        }

        private static void LogToFile(string text)
        {
            try
            {
                var filePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, Constants.Application.EXCEPTIONLOG_FILENAME);

                var existing = File.ReadAllText(filePath);
                existing += text;

                File.WriteAllText(filePath, existing);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
    }
}
