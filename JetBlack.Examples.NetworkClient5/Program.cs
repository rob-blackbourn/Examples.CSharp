using System;
using System.Net.Sockets;
using System.Text;
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

            var client = new TcpClient();
            client.Connect(address, port);
            var subject = client.ToSubject(cts.Token);

            subject.Subscribe(buf => Console.WriteLine(Encoding.UTF8.GetString(buf)), cts.Token);

            for (var line = Console.ReadLine(); !string.IsNullOrEmpty(line); line = Console.ReadLine())
                subject.OnNext(Encoding.UTF8.GetBytes(line));
        }
    }
}
