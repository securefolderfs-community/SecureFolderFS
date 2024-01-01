using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace SecureFolderFS.Core.WebDav.Helpers
{
    internal static class PortHelpers
    {
        public static bool IsPortAvailable(int port)
        {
            return !GetUnavailablePorts().Contains(port);
        }

        public static int GetNextAvailablePort(int startingPort)
        {
            var unavailablePorts = GetUnavailablePorts().ToList();
            for (var i = startingPort; i <= ushort.MaxValue; i++)
                if (!unavailablePorts.Contains(i))
                    return i;

            throw new InvalidOperationException("No available ports.");
        }

        private static IEnumerable<int> GetUnavailablePorts()
        {
            if (OperatingSystem.IsAndroid())
                return Enumerable.Empty<int>(); // TODO(u android)
            
            var properties = IPGlobalProperties.GetIPGlobalProperties();
            return properties.GetActiveTcpConnections().Select(x => x.LocalEndPoint.Port)
                .Concat(properties.GetActiveTcpListeners().Select(x => x.Port))
                .Concat(properties.GetActiveUdpListeners().Select(x => x.Port));
        }
    }
}