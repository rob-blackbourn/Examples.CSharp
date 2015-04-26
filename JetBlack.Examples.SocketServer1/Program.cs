using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace JetBlack.Examples.SocketServer1
{
    class Program
    {
        public static void Main(string[] args)
        {
            string[] splitArgs = null;
            if (args.Length != 1 || (splitArgs = args[0].Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries)).Length != 2)
            {
                Console.WriteLine("usage: EchoServer <hostname>:<port>");
                Console.WriteLine("example:");
                Console.WriteLine("    > EchoServer 127.0.0.1:9211");
                Environment.Exit(-1);
            }

            var address = IPAddress.Parse(splitArgs[0]);
            var port = int.Parse(splitArgs[1]);

            using (var listener = new EchoServer(address, port))
            {
                var cts = new CancellationTokenSource();
                var task = Task.Factory.StartNew(() => listener.Dispatch(cts.Token), cts.Token);

                Console.WriteLine("Press <ENTER> to quit");
                Console.ReadLine();

                cts.Cancel();
                task.Wait(cts.Token);
            }
        }
    }

    class EchoServer : IDisposable
    {
        private readonly Socket _socket;

        public EchoServer(IPAddress address, int port)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(address, port));
        }

        public void Dispatch(CancellationToken token)
        {
            _socket.Listen(10);

            while (!token.IsCancellationRequested)
            {
                var client = _socket.Accept();

                Task.Factory.StartNew(() =>
                {
                    using (var echoClient = new EchoClient(client))
                        echoClient.Dispatch(token);
                }, token);
            }
        }

        public void Dispose()
        {
            _socket.Close();
        }
    }

    class EchoClient : IDisposable
    {
        private readonly Socket _socket;

        public EchoClient(Socket socket)
        {
            _socket = socket;
        }

        public void Dispatch(CancellationToken token)
        {
            var buffer = new byte[1024];
            while (!token.IsCancellationRequested)
            {
                var read = _socket.Receive(buffer);
                if (read == 0)
                    break;
                var written = 0;
                while (written < read)
                    written += _socket.Send(buffer, written, read - written, SocketFlags.None);
            }
        }

        public void Dispose()
        {
            _socket.Close();
        }
    }
}
