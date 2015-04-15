using System;
using System.IO;
using System.Net.Sockets;

namespace JetBlack.Examples.EchoClient2
{
    class Program
    {
        public static void Main(string[] args)
        {
            string[] splitArgs = null;
            if (args.Length != 1 || (splitArgs = args[0].Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries)).Length != 2)
            {
                Console.WriteLine("usage: EchoClient <hostname>:<port>");
                Console.WriteLine("example:");
                Console.WriteLine("    > EchoClient localhost:9211");
                Environment.Exit(-1);
            }

            var client = new EchoClient(splitArgs[0], int.Parse(splitArgs[1]));
            client.Dispatch();
            client.Dispose();
        }
    }

    class EchoClient : IDisposable
    {
        private readonly TcpClient _tcpClient;

        public EchoClient(string hostname, int port)
        {
            _tcpClient = new TcpClient(hostname, port);
        }

        public void Dispatch()
        {
            using (var stream = _tcpClient.GetStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream) {AutoFlush = true})
                for (var line = Console.ReadLine(); !string.IsNullOrEmpty(line); line = Console.ReadLine())
                {
                    writer.WriteLine(line);
                    Console.WriteLine(reader.ReadLine());
                }
        }

        public void Dispose()
        {
            _tcpClient.Close();
        }
    }
}
