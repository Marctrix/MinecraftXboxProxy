using System.Linq;
using System.Net;

namespace MinecraftXboxProxy.Console
{
    class Program
    {
        const int gamePort = 19132;

        static IPAddress xBoxIp;
        static IPAddress serverIp;

        static void Main(string[] args)
        {
            if (args.Length >= 1)
                xBoxIp = IPAddressFromArgs(args[0]);

            if (args.Length >= 2)
                serverIp = IPAddressFromArgs(args[1]);

            var proxy = new MinecraftXboxProxyService(xBoxIp, serverIp, gamePort);
            proxy.Start();

            //create a new client
            //type ahead :-)
            string read;
            do
            {
                read = System.Console.ReadLine();
            } while (read != "quit");
        }

        static IPAddress IPAddressFromArgs(string arg)
        {
            return Dns.GetHostEntry(arg).AddressList.First(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        }
    }
}
