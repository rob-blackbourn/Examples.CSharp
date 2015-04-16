using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using JetBlack.Examples.RxNetwork;

namespace JetBlack.Examples.RxNetworkClient
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

            var client = new TcpClient(address, port);
            var subject = client.ToFrameSubject(cts.Token);

            subject.Subscribe(
                buf => Console.WriteLine("OnNext: {0}", Encoding.UTF8.GetString(buf)),
                error => Console.WriteLine("OnError: {0}\r\n{1}", error.Message, error.StackTrace),
                () => Console.WriteLine("OnCompleted"), cts.Token);

            for (var line = Console.ReadLine(); !string.IsNullOrEmpty(line); line = Console.ReadLine())
                subject.OnNext(Encoding.UTF8.GetBytes(line));
        }
    }
}
