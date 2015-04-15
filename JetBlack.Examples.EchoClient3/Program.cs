using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace JetBlack.Examples.EchoClient3
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

            using (var client = new EchoClient())
            {
                client.Dispatch(splitArgs[0], int.Parse(splitArgs[1])).Wait();
            }
        }
    }

    class EchoClient : IDisposable
    {
        private readonly TcpClient _tcpClient;

        public EchoClient()
        {
            _tcpClient = new TcpClient();
        }

        public async Task<CancellationToken> Dispatch(string hostname, int port)
        {
            await _tcpClient.ConnectAsync(hostname, port);

            var cts = new CancellationTokenSource();

            using (var stream = _tcpClient.GetStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream) {AutoFlush = true})
                for (var line = Console.ReadLine(); !string.IsNullOrEmpty(line); line = Console.ReadLine())
                {
                    await writer.WriteLineAsync(line);
                    Console.WriteLine(await reader.ReadLineAsync());
                }
            return cts.Token;
        }

        public void Dispose()
        {
            _tcpClient.Close();
        }
    }
}
