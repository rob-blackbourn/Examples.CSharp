using System;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JetBlack.Examples.SocketServer3
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

        public async void Dispatch(CancellationToken token)
        {
            _socket.Listen(10);

            while (!token.IsCancellationRequested)
            {
                var client = await _socket.AcceptAsync();

                var clientSubject = client.ToSenderReceiver(1024, SocketFlags.None);
                clientSubject.SubscribeOn(TaskPoolScheduler.Default).Subscribe(clientSubject, token);
            }
        }

        public void Dispose()
        {
            _socket.Close();
        }
    }
}
