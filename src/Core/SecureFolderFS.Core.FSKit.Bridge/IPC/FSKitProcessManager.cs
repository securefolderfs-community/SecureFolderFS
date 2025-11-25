using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract;

namespace SecureFolderFS.Core.FSKit.Bridge.IPC
{
    /// <summary>
    /// Manages the lifecycle of the FSKit service process.
    /// </summary>
    internal sealed class FSKitProcessManager : IDisposable
    {
        private Process? _fskitProcess;
        private readonly string _fskitAppPath;
        private readonly SemaphoreSlim _startLock = new(1, 1);
        private bool _isDisposed;

        public FSKitProcessManager(string? customPath = null)
        {
            if (!string.IsNullOrEmpty(customPath))
            {
                _fskitAppPath = customPath;
            }
            else
            {
                // Try to locate the FSKit service in common locations
                var possiblePaths = new[]
                {
                    // Development build paths
                    Path.Combine(AbstractPathHelpers.GetParentPath(AppContext.BaseDirectory, 6), "Core", "SecureFolderFS.Core.FSKit", "bin", "Debug", "net10.0-macos", "osx-arm64", "SecureFolderFS.Core.FSKit.app"),
                    Path.Combine(AbstractPathHelpers.GetParentPath(AppContext.BaseDirectory, 6), "Core", "SecureFolderFS.Core.FSKit", "bin", "Release", "net10.0-macos", "osx-arm64", "SecureFolderFS.Core.FSKit.app"),

                    // Installed paths
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SecureFolderFS", "FSKit", "SecureFolderFS.Core.FSKit.app"),
                    "/Applications/SecureFolderFS.Core.FSKit.app"
                };

                foreach (var path in possiblePaths)
                {
                    var resolved = Path.GetFullPath(path);
                    if (Directory.Exists(resolved) && resolved.EndsWith(".app", StringComparison.OrdinalIgnoreCase))
                    {
                        _fskitAppPath = resolved;
                        Debug.WriteLine($"FSKit Process Manager: Found FSKit service at {_fskitAppPath}");
                        break;
                    }
                }

                // Fallback to hardcoded path
                _fskitAppPath ??= "/Applications/SecureFolderFS.Core.FSKit.app";
            }

            Debug.WriteLine($"FSKit Process Manager: Using FSKit service path: {_fskitAppPath}");
        }

        public bool IsRunning => _fskitProcess != null && !_fskitProcess.HasExited;

        public async Task<bool> StartServiceAsync(CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(FSKitProcessManager));

            await _startLock.WaitAsync(cancellationToken);
            try
            {
                // Check if already running
                if (IsRunning)
                {
                    Debug.WriteLine("FSKit service is already running");
                    return true;
                }

                // Clean up any previous process
                if (_fskitProcess != null)
                {
                    try
                    {
                        _fskitProcess.Dispose();
                    }
                    catch { /* Ignore */ }
                    _fskitProcess = null;
                }

                // Determine executable path
                string execPath;
                bool useDirectExec = false;

                if (Directory.Exists(_fskitAppPath) && _fskitAppPath.EndsWith(".app", StringComparison.OrdinalIgnoreCase))
                {
                    // Look for executable in Contents/MacOS
                    var contentsMacOs = Path.Combine(_fskitAppPath, "Contents", "MacOS");
                    if (Directory.Exists(contentsMacOs))
                    {
                        var files = Directory.GetFiles(contentsMacOs);
                        var appName = Path.GetFileNameWithoutExtension(_fskitAppPath);
                        var candidate = Array.Find(files, f => Path.GetFileName(f).Equals(appName, StringComparison.OrdinalIgnoreCase))
                                       ?? (files.Length > 0 ? files[0] : null);

                        if (candidate != null && File.Exists(candidate))
                        {
                            execPath = candidate;
                            useDirectExec = true;
                        }
                        else
                        {
                            execPath = _fskitAppPath;
                        }
                    }
                    else
                    {
                        execPath = _fskitAppPath;
                    }
                }
                else if (File.Exists(_fskitAppPath))
                {
                    execPath = _fskitAppPath;
                    useDirectExec = true;
                }
                else
                {
                    Debug.WriteLine($"FSKit service not found at '{_fskitAppPath}'");
                    return false;
                }

                // Start the process
                if (useDirectExec)
                {
                    // Direct execution of the binary
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

                    _fskitProcess.OutputDataReceived += (_, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                            Debug.WriteLine($"[FSKit] {e.Data}");
                    };
                    _fskitProcess.ErrorDataReceived += (_, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                            Debug.WriteLine($"[FSKit Error] {e.Data}");
                    };

                    var started = _fskitProcess.Start();
                    if (started)
                    {
                        _fskitProcess.BeginOutputReadLine();
                        _fskitProcess.BeginErrorReadLine();

                        // Give the service time to initialize
                        await Task.Delay(1000, cancellationToken);

                        if (_fskitProcess.HasExited)
                        {
                            Debug.WriteLine($"FSKit service exited immediately with code {_fskitProcess.ExitCode}");
                            return false;
                        }

                        Debug.WriteLine("FSKit service started successfully (direct execution)");
                        return true;
                    }

                    Debug.WriteLine("Failed to start FSKit service (Process.Start returned false)");
                    return false;
                }
                else
                {
                    // Use 'open -a' for .app bundles
                    _fskitProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "open",
                            Arguments = $"-a \"{execPath}\"",
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true
                        }
                    };

                    _fskitProcess.OutputDataReceived += (_, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                            Debug.WriteLine($"[FSKit-open] {e.Data}");
                    };
                    _fskitProcess.ErrorDataReceived += (_, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                            Debug.WriteLine($"[FSKit-open Error] {e.Data}");
                    };

                    var started = _fskitProcess.Start();
                    if (started)
                    {
                        _fskitProcess.BeginOutputReadLine();
                        _fskitProcess.BeginErrorReadLine();

                        // Give the app time to launch
                        await Task.Delay(1500, cancellationToken);

                        Debug.WriteLine("FSKit service started successfully (using 'open')");
                        return true;
                    }

                    Debug.WriteLine("Failed to start FSKit service using 'open'");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to start FSKit service: {ex.Message}");
                return false;
            }
            finally
            {
                _startLock.Release();
            }
        }

        public async Task StopServiceAsync()
        {
            if (_isDisposed)
                return;

            await _startLock.WaitAsync();
            try
            {
                if (_fskitProcess != null && !_fskitProcess.HasExited)
                {
                    try
                    {
                        Debug.WriteLine("Stopping FSKit service...");

                        // Try graceful shutdown first
                        _fskitProcess.Kill(entireProcessTree: true);

                        var exited = _fskitProcess.WaitForExit(5000);
                        if (!exited)
                        {
                            Debug.WriteLine("FSKit service did not exit gracefully, forcing termination");
                            _fskitProcess.Kill();
                        }
                        else
                        {
                            Debug.WriteLine("FSKit service stopped successfully");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error stopping FSKit service: {ex.Message}");
                    }
                }

                _fskitProcess?.Dispose();
                _fskitProcess = null;
            }
            finally
            {
                _startLock.Release();
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            StopServiceAsync().GetAwaiter().GetResult();
            _startLock.Dispose();
        }
    }
}

