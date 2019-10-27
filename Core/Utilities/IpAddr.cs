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

        public static bool Equals(string lhs, string rhs)
        {
            return lhs == rhs || lhs == $"::ffff:{rhs}" || rhs == $"::ffff:{lhs}";
        }

        public static bool EqualsMine(string Address)
        {
            return Mine() == Address
                || Mine() == $"::ffff:{Address}"
                || $"::ffff:{Mine()}" == Address
                || MineExternal() == Address
                || MineExternal() == $"::ffff:{Address}"
                || $"::ffff:{MineExternal()}" == Address;
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
    }
}
