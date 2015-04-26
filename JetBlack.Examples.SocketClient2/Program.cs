using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JetBlack.Examples.SocketClient2
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

            var cts = new CancellationTokenSource();

            using (var client = new EchoClient())
            {
                client.Dispatch(splitArgs[0], int.Parse(splitArgs[1]), cts.Token).Wait(cts.Token);
            }
        }
    }

    class EchoClient : IDisposable
    {
        private readonly Socket _socket;

        public EchoClient()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public async Task Dispatch(string hostname, int port, CancellationToken token)
        {
            await _socket.ConnectAsync(hostname, port);

            for (var line = Console.ReadLine(); !token.IsCancellationRequested && !string.IsNullOrEmpty(line); line = Console.ReadLine())
            {
                var writeBuffer = Encoding.UTF8.GetBytes(line);
                var written = 0;
                while (written < writeBuffer.Length)
                    written += await _socket.SendAsync(writeBuffer, written, writeBuffer.Length - written, SocketFlags.None);

                var readBuffer = new byte[written];
                var read = 0;
                while (read < written)
                    read += await _socket.ReceiveAsync(readBuffer, read, written - read, SocketFlags.None);

                Console.WriteLine("Read: " + Encoding.UTF8.GetString(readBuffer));
            }
        }

        public void Dispose()
        {
            _socket.Close();
        }
    }
}
