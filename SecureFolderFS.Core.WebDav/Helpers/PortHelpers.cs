using System;
using System.Net;
using System.Net.Sockets;

namespace SecureFolderFS.Core.WebDav.Helpers
{
    internal static class PortHelpers
    {
        public static bool IsPortAvailable(int port)
        {
            try
            {
                using var client = new TcpClient();
                client.Connect("localhost", port);
                client.Close();
            }
            catch (Exception)
            {
                return true;
            }

            return false;
        }

        public static int GetAvailablePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();

            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}