using System.Linq;
using System.Net;

namespace Core.Utilities
{
    public static class IpAddr
    {
        public static string Mine()
        {
            return Dns.GetHostEntry(Dns.GetHostName())
               .AddressList
               .First(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
               .ToString();
        }
    }
}
