using System.Net.Sockets;

namespace Core.Utilities
{
    public static class TcpClientExtensions
    {
        public static string Ip(this TcpClient self)
        {
            // raw cutting in an endpoint... probably bugged because it expects ipv4
            // address in this format [::ffff:xxx.yyy.zzz.aaa]:18910
            string raw = self.Client.RemoteEndPoint.AddressFamily .ToString().Split(':')[3];
            return raw.Remove(raw.Length - 1);
        }
    }
}
