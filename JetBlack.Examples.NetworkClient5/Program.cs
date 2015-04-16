using System;
using System.Threading;

namespace JetBlack.Examples.NetworkClient5
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] splitArgs = null;
            if (args.Length != 1 || (splitArgs = args[0].Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries)).Length != 2)
            {
                Console.WriteLine("usage: EchoClient <hostname>:<port>");
                Console.WriteLine("example:");
                Console.WriteLine("    > EchoClient localhost:9211");
                Environment.Exit(-1);
            }

            var address = splitArgs[0];
            var port = int.Parse(splitArgs[1]);

            var cts = new CancellationTokenSource();

            var client = new EchoClient();
            client.Dispatch(address, port, cts.Token).Wait(cts.Token);
        }
    }
}
