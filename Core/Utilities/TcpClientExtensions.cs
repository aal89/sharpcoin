using System.Net.Sockets;

namespace Core.Utilities
{
    public static class TcpClientExtensions
    {
        public static string Ip(this TcpClient self)
        {
            return self.Client.RemoteEndPoint.ToString().Split(":")[0];
        }
    }
}
