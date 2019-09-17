using System.Linq;
using System.Net;

namespace Core.Utilities
{
    public static class IpAddr
    {
        private static string Address;

        public static void Set(string Address)
        {
            IpAddr.Address = Address;
        }

        public static string Mine()
        {
            return Address ?? Dns.GetHostEntry(Dns.GetHostName())
               .AddressList
               .First(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
               .ToString();
        }
        
        public static IPAddress MineAsObject()
        {
            return IPAddress.Parse(Mine());
        }
    }
}
