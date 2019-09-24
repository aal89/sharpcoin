using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Open.Nat;

namespace Core.Utilities
{
    public static class IpAddr
    {
        private static string Address;
        private static NatDevice Device;

        public static void Set(string Address)
        {
            IpAddr.Address = Address;
        }

        public static void Set(NatDevice Device)
        {
            IpAddr.Device = Device;
        }

        public static string Mine()
        {
            return Address ?? Dns.GetHostEntry(Dns.GetHostName())
               .AddressList
               .First(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
               .ToString();
        }

        public static string MineExternal()
        {
            return Device != null ? Task.Run(async () => await Device.GetExternalIPAsync()).Result.ToString() : "0.0.0.0";
        }
        
        public static IPAddress MineAsObject()
        {
            return IPAddress.Parse(Mine());
        }
    }
}
