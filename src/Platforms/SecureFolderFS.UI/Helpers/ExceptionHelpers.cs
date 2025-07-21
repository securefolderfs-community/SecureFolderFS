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

        public static void WriteSessionFile(string appDirectory, string exceptionMessage)
        {
            try
            {
                var filePath = Path.Combine(appDirectory, Constants.Application.SESSION_EXCEPTION_FILENAME);
                File.WriteAllText(filePath, exceptionMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Debugger.Break();
            }
        }

        public static string? RetrieveSessionFile(string appDirectory)
        {
            try
            {
                var filePath = Path.Combine(appDirectory, Constants.Application.SESSION_EXCEPTION_FILENAME);
                var text = File.ReadAllText(filePath);
                File.WriteAllText(filePath, string.Empty);

                return string.IsNullOrEmpty(text) ? null : text;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Debugger.Break();

                return null;
            }
        }

        public static void WriteAggregateFile(string appDirectory, Exception? exception)
        {
            var exceptionMessage = FormatException(exception);
            if (exceptionMessage is null)
                return;
            
            WriteAggregateFile(appDirectory, exceptionMessage);
        }

        public static void WriteAggregateFile(string appDirectory, string exceptionMessage)
        {
            try
            {
                var filePath = Path.Combine(appDirectory, Constants.Application.EXCEPTION_LOG_FILENAME);
                File.AppendAllText(filePath, $"\n\n\t\tEXCEPTION EVENT: {DateTime.Now}\n{exceptionMessage}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Debugger.Break();
            }
        }
    }
}
