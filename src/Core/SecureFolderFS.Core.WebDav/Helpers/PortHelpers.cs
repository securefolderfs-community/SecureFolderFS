using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace SecureFolderFS.Core.WebDav.Helpers
{
    internal static class PortHelpers
    {
        /// <summary>
        /// Checks if the specified <paramref name="port"/> is available for binding by attempting
        /// to listen on it or verifying it is not in use.
        /// </summary>
        /// <param name="port">The port number to check for availability.</param>
        /// <returns>
        /// <c>true</c> if the port is available for use; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsPortAvailable(int port)
        {
            if (OperatingSystem.IsAndroid())
                return CanBindPort(port);

            try
            {
                return !GetUnavailablePorts().Contains(port);
            }
            catch (Exception)
            {
                // Enumeration may be unsupported on this platform
                return CanBindPort(port);
            }
        }

        /// <summary>
        /// Finds and returns the next available network port starting from the specified <paramref name="startingPort"/>.
        /// </summary>
        /// <param name="startingPort">The port number from which to start searching for an available port.</param>
        /// <returns>The next available port number.</returns>
        public static int GetNextAvailablePort(int startingPort)
        {
            if (!OperatingSystem.IsAndroid())
            {
                try
                {
                    var unavailablePorts = GetUnavailablePorts().ToHashSet();
                    for (var i = startingPort; i <= ushort.MaxValue; i++)
                    {
                        if (!unavailablePorts.Contains(i))
                            return i;
                    }

                    throw new InvalidOperationException("No available ports.");
                }
                catch (Exception ex) when (ex is not InvalidOperationException)
                {
                    // Enumeration is unsupported on this platform
                }
            }

            for (var i = startingPort; i <= ushort.MaxValue; i++)
            {
                if (CanBindPort(i))
                    return i;
            }

            throw new InvalidOperationException("No available ports.");
        }

        /// <summary>
        /// Determines whether <paramref name="port"/> can be bound by attempting to listen on it.
        /// Works on every platform, including those where <see cref="IPGlobalProperties"/> enumeration is unavailable (e.g. Android).
        /// </summary>
        private static bool CanBindPort(int port)
        {
            try
            {
                using var listener = new TcpListener(IPAddress.Loopback, port);
                listener.Start();
                listener.Stop();
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
        }

        private static IEnumerable<int> GetUnavailablePorts()
        {
            var properties = IPGlobalProperties.GetIPGlobalProperties();
            return properties.GetActiveTcpConnections().Select(x => x.LocalEndPoint.Port)
                .Concat(properties.GetActiveTcpListeners().Select(x => x.Port))
                .Concat(properties.GetActiveUdpListeners().Select(x => x.Port));
        }
    }
}
