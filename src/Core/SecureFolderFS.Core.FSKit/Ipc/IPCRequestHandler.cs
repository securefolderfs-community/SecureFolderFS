using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.FSKit.Callbacks;
using SecureFolderFS.Sdk.Ipc;
using SecureFolderFS.Sdk.Ipc.Extensions;

namespace SecureFolderFS.Core.FSKit.Ipc
{
    /// <summary>
    /// Handles IPC requests and manages file system operations.
    /// </summary>
    internal sealed class IPCRequestHandler
    {
#pragma warning disable APL0002
        private readonly Dictionary<string, MacOsFileSystemInstance> _activeMounts = new();
#pragma warning restore APL0002
        private readonly SemaphoreSlim _mountLock = new(1, 1);

        public async Task<IpcResponse> HandleRequestAsync(IpcRequest request, CancellationToken cancellationToken)
        {
            try
            {
                return request.Command switch
                {
                    "ping" => HandlePing(request),
                    "mount" => await HandleMountAsync(request, cancellationToken),
                    "unmount" => await HandleUnmountAsync(request, cancellationToken),
                    "get_status" => HandleGetStatus(request),
                    _ => MessageExtensions.CreateErrorResponse(request.RequestId, "Unknown command", $"Command '{request.Command}' is not supported")
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FSKit IPC: Error handling request {request.RequestId}: {ex.Message}");
                return MessageExtensions.CreateErrorResponse(request.RequestId, ex.Message, "Internal error");
            }
        }

        private IpcResponse HandlePing(IpcRequest request)
        {
            return MessageExtensions.CreateSuccessResponse(
                request.RequestId,
                "pong",
                new Dictionary<string, object>
                {
                    ["timestamp"] = DateTime.UtcNow.ToString("O"),
                    ["service"] = "FSKit"
                });
        }

        private async Task<IpcResponse> HandleMountAsync(IpcRequest request, CancellationToken cancellationToken)
        {
            await _mountLock.WaitAsync(cancellationToken);
            try
            {
                // Extract parameters
                if (request.Parameters == null)
                    return MessageExtensions.CreateErrorResponse(request.RequestId, "Missing parameters", "Mount requires volumeName and readOnly parameters");

                // mountPoint is optional - FSKit will choose it
                if (!request.Parameters.TryGetValue("volumeName", out var volumeNameObj) ||
                    !request.Parameters.TryGetValue("readOnly", out var readOnlyObj))
                {
                    return MessageExtensions.CreateErrorResponse(request.RequestId, "Invalid parameters", "Mount requires volumeName and readOnly parameters");
                }

                // Extract values from objects (handles JsonElement deserialization)
                var volumeName = ExtractString(volumeNameObj);
                var readOnly = ExtractBool(readOnlyObj);

                if (string.IsNullOrEmpty(volumeName))
                    return MessageExtensions.CreateErrorResponse(request.RequestId, "Invalid parameters", "volumeName cannot be empty");

                Console.WriteLine($"FSKit IPC: Mount request - volumeName: {volumeName}, readOnly: {readOnly}");

                // Create and mount the file system
#pragma warning disable APL0002
                var fileSystem = new MacOsFileSystem();
                var instance = new MacOsFileSystemInstance(fileSystem, volumeName);
#pragma warning restore APL0002

                try
                {
                    // Mount the file system - FSKit will determine the mount point
                    var mountPoint = await instance.MountAsync(readOnly, cancellationToken);

                    if (string.IsNullOrEmpty(mountPoint))
                    {
                        return MessageExtensions.CreateErrorResponse(request.RequestId, "Mount failed", "FSKit did not return a mount point");
                    }

                    // Track the mounted file system
                    _activeMounts[mountPoint] = instance;

                    Console.WriteLine($"FSKit IPC: File system mounted at {mountPoint}");

                    return MessageExtensions.CreateSuccessResponse(
                        request.RequestId,
                        "File system mounted successfully",
                        new Dictionary<string, object>
                        {
                            ["mountPoint"] = mountPoint,
                            ["volumeName"] = volumeName
                        });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"FSKit IPC: Mount failed: {ex.Message}");
                    instance.Dispose();
                    return MessageExtensions.CreateErrorResponse(request.RequestId, "Mount failed", ex.Message);
                }
            }
            finally
            {
                _mountLock.Release();
            }
        }

        private async Task<IpcResponse> HandleUnmountAsync(IpcRequest request, CancellationToken cancellationToken)
        {
            await _mountLock.WaitAsync(cancellationToken);
            try
            {
                // Extract parameters
                if (request.Parameters == null || !request.Parameters.TryGetValue("mountPoint", out var mountPointObj))
                    return MessageExtensions.CreateErrorResponse(request.RequestId, "Missing parameter", "Unmount requires mountPoint parameter");

                var mountPoint = ExtractString(mountPointObj);
                if (string.IsNullOrEmpty(mountPoint))
                    return MessageExtensions.CreateErrorResponse(request.RequestId, "Invalid parameter", "mountPoint cannot be empty");

                Console.WriteLine($"FSKit IPC: Unmount request - mountPoint: {mountPoint}");

                // Check if mounted
                if (!_activeMounts.TryGetValue(mountPoint, out var instance))
                {
                    return MessageExtensions.CreateErrorResponse(request.RequestId, "Not mounted", $"No file system is mounted at {mountPoint}");
                }

                try
                {
                    // Unmount the file system
                    await instance.UnmountAsync(cancellationToken);
                    _activeMounts.Remove(mountPoint);

                    Console.WriteLine($"FSKit IPC: File system unmounted from {mountPoint}");

                    return MessageExtensions.CreateSuccessResponse(
                        request.RequestId,
                        "File system unmounted successfully",
                        new Dictionary<string, object>
                        {
                            ["mountPoint"] = mountPoint
                        });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"FSKit IPC: Unmount failed: {ex.Message}");
                    return MessageExtensions.CreateErrorResponse(request.RequestId, "Unmount failed", ex.Message);
                }
                finally
                {
                    instance.Dispose();
                }
            }
            finally
            {
                _mountLock.Release();
            }
        }

        private IpcResponse HandleGetStatus(IpcRequest request)
        {
            return MessageExtensions.CreateSuccessResponse(
                request.RequestId,
                "Service is running",
                new Dictionary<string, object>
                {
                    ["activeMounts"] = _activeMounts.Count,
                    ["service"] = "FSKit",
                    ["version"] = "1.0.0"
                });
        }

        /// <summary>
        /// Extracts a string value from an object, handling JsonElement conversion.
        /// </summary>
        private static string? ExtractString(object? obj)
        {
            if (obj == null)
                return null;

            // Handle direct string
            if (obj is string str)
                return str;

            // Handle JsonElement (when deserialized from JSON)
            if (obj is JsonElement element)
            {
                if (element.ValueKind == JsonValueKind.String)
                    return element.GetString();
            }

            // Fallback to ToString()
            return obj.ToString();
        }

        /// <summary>
        /// Extracts a bool value from an object, handling JsonElement conversion.
        /// </summary>
        private static bool ExtractBool(object? obj)
        {
            if (obj == null)
                return false;

            // Handle direct bool
            if (obj is bool boolVal)
                return boolVal;

            // Handle JsonElement (when deserialized from JSON)
            if (obj is not JsonElement element)
                return Convert.ToBoolean(obj);

            if (element.ValueKind == JsonValueKind.True)
                return true;

            if (element.ValueKind == JsonValueKind.False)
                return false;

            // Try to parse as boolean
            if (element.ValueKind == JsonValueKind.String)
            {
                var strVal = element.GetString();
                if (bool.TryParse(strVal, out var parsed))
                    return parsed;
            }

            // Fallback to conversion
            return Convert.ToBoolean(obj);
        }

        public void Dispose()
        {
            _mountLock.Dispose();
        }
    }
}

