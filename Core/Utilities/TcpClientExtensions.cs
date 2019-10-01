using System.Net;
using System.Net.Sockets;

namespace Core.Utilities
{
    public static class TcpClientExtensions
    {
        public static string Ip(this TcpClient self)
        {
            return ((IPEndPoint)self.Client.RemoteEndPoint).Address.ToString();
        }
    }
}
