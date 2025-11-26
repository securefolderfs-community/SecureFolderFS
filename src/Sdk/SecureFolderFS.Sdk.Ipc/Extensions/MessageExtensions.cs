using System;
using System.Collections.Generic;

namespace SecureFolderFS.Sdk.Ipc.Extensions
{
    public static class MessageExtensions
    {
        /// <summary>
        /// Determines whether the given <see cref="IpcResponse"/> object indicates a successful operation.
        /// </summary>
        /// <param name="response">The <see cref="IpcResponse"/> instance to evaluate.</param>
        /// <returns>True if the response status is "success"; otherwise, false.</returns>
        public static bool IsSuccess(this IpcResponse response)
        {
            return response.Status.Equals("success", StringComparison.OrdinalIgnoreCase);
        }

        // TODO: Convert to extension members

        public static IpcRequest CreateGetStatusRequest()
        {
            return new IpcRequest()
            {
                Command = Constants.Commands.GET_STATUS
            };
        }

        public static IpcRequest CreatePingRequest()
        {
            return new IpcRequest()
            {
                Command = Constants.Commands.PING
            };
        }

        public static IpcResponse CreateSuccessResponse(string requestId, string? message = null, Dictionary<string, object>? data = null)
        {
            return new IpcResponse
            {
                RequestId = requestId,
                Status = "success",
                Message = message,
                Data = data
            };
        }

        public static IpcResponse CreateErrorResponse(string requestId, string error, string? message = null)
        {
            return new IpcResponse
            {
                RequestId = requestId,
                Status = "error",
                Error = error,
                Message = message
            };
        }

        public static IpcRequest CreateMountRequest(string? mountPoint, string volumeName, bool readOnly)
        {
            var parameters = new Dictionary<string, object>()
            {
                ["volumeName"] = volumeName,
                ["readOnly"] = readOnly
            };

            // Add mountPoint only if provided (FSKit can choose mount point if null)
            if (mountPoint != null)
            {
                parameters["mountPoint"] = mountPoint;
            }

            return new IpcRequest
            {
                Command = Sdk.Ipc.Constants.Commands.MOUNT,
                Parameters = parameters
            };
        }

        public static IpcRequest CreateUnmountRequest(string mountPoint)
        {
            return new IpcRequest
            {
                Command = Sdk.Ipc.Constants.Commands.UNMOUNT,
                Parameters = new Dictionary<string, object>()
                {
                    ["mountPoint"] = mountPoint
                }
            };
        }
    }
}
