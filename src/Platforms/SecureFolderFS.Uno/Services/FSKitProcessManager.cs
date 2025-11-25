using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.Uno.Services;

public class FSKitProcessManager : IDisposable
{
    private Process? _fskitProcess;
    private readonly string _fskitAppPath;

    public FSKitProcessManager()
    {
        // Adjust path to match your build output
        // Prefer pointing directly to the binary inside the .app bundle if available.
        // Default to the typical build output .app path (user-specific in this repo).
        
    }

    public async Task<bool> StartServiceAsync()
    {
        try
        {
            var isStarted = SafetyHelpers.NoFailureResult(() => _fskitProcess?.HasExited == false);
            if (_fskitProcess != null && isStarted)
                return true;

            // Determine if the path points to a .app bundle or an executable and pick the right launch strategy
            string execPath = _fskitAppPath;

            if (Directory.Exists(_fskitAppPath) && _fskitAppPath.EndsWith(".app", StringComparison.OrdinalIgnoreCase))
            {
                // Look for an executable in Contents/MacOS
                var contentsMacOs = Path.Combine(_fskitAppPath, "Contents", "MacOS");
                if (Directory.Exists(contentsMacOs))
                {
                    var files = Directory.GetFiles(contentsMacOs);
                    // Prefer a file that has the same name as the app directory, otherwise pick the first file
                    var appName = Path.GetFileNameWithoutExtension(_fskitAppPath);
                    var candidate = Array.Find(files, f => Path.GetFileName(f).Equals(appName, StringComparison.OrdinalIgnoreCase))
                                   ?? (files.Length > 0 ? files[0] : null);

                    if (candidate is not null)
                        execPath = candidate;
                }
            }

            // If execPath points to an actual file, start it directly and enable output redirection
            if (File.Exists(execPath))
            {
                _fskitProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = execPath,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };

                _fskitProcess.OutputDataReceived += (_, e) => Debug.WriteLine($"FSKit: {e.Data}");
                _fskitProcess.ErrorDataReceived += (_, e) => Debug.WriteLine($"FSKit Error: {e.Data}");

                var started = _fskitProcess.Start();

                // Only begin reading output if the process actually started
                if (started)
                {
                    try
                    {
                        _fskitProcess.BeginOutputReadLine();
                        _fskitProcess.BeginErrorReadLine();
                    }
                    catch (InvalidOperationException ioe)
                    {
                        Debug.WriteLine($"BeginReadLine failed: {ioe.Message}");
                    }

                    // Give the service time to initialize
                    await Task.Delay(1000);

                    return true;
                }

                Debug.WriteLine("Failed to start FSKit executable (Process.Start returned false).");
                return false;
            }

            // If we couldn't find an executable but the .app bundle exists, try using `open -a` as a fallback
            if (Directory.Exists(_fskitAppPath) && _fskitAppPath.EndsWith(".app", StringComparison.OrdinalIgnoreCase))
            {
                _fskitProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "open",
                        Arguments = $"-a \"{_fskitAppPath}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };

                _fskitProcess.OutputDataReceived += (_, e) => Debug.WriteLine($"FSKit(open): {e.Data}");
                _fskitProcess.ErrorDataReceived += (_, e) => Debug.WriteLine($"FSKit Error(open): {e.Data}");

                var started = _fskitProcess.Start();
                if (started)
                {
                    try
                    {
                        _fskitProcess.BeginOutputReadLine();
                        _fskitProcess.BeginErrorReadLine();
                    }
                    catch (InvalidOperationException ioe)
                    {
                        Debug.WriteLine($"BeginReadLine for open failed: {ioe.Message}");
                    }

                    await Task.Delay(500);
                    // Note: `open` exits quickly, the actual app continues as a separate process.
                    return true;
                }

                Debug.WriteLine("Failed to start FSKit .app using `open`.");
                return false;
            }

            Debug.WriteLine($"FSKit executable or .app not found at '{_fskitAppPath}'.");
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to start FSKit service: {ex.Message}");
            return false;
        }
    }

    public void StopService()
    {
        try
        {
            if (_fskitProcess != null && !_fskitProcess.HasExited)
            {
                try
                {
                    _fskitProcess.Kill(entireProcessTree: true);
                }
                catch
                {
                    // Fallback to non-tree kill for older runtimes
                    _fskitProcess.Kill();
                }

                _fskitProcess.WaitForExit(5000);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error stopping FSKit service: {ex.Message}");
        }
    }

    public void Dispose()
    {
        StopService();
        _fskitProcess?.Dispose();
    }
}
