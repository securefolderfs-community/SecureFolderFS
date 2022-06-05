using System;
using System.Diagnostics;
using System.IO;
using Windows.Storage;

namespace SecureFolderFS.WinUI.Helpers
{
    internal static class ExceptionHelpers
    {
        public static string? FormatException(Exception? ex)
        {
            if (ex is null)
            {
                return null;
            }

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

        public static void LogExceptionToFile(string? formattedException)
        {
            if (formattedException is null)
            {
                return;
            }

            try
            {
                var filePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, Constants.Application.EXCEPTIONLOG_FILENAME);

                var existing = File.ReadAllText(filePath);
                existing += formattedException;

                File.WriteAllText(filePath, existing);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
