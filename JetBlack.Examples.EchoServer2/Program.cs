using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace JetBlack.Examples.EchoServer2
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
        private readonly TcpListener _tcpListener;

        public EchoServer(IPAddress address, int port)
        {
            _tcpListener = new TcpListener(address, port);
        }

        public void Dispatch(CancellationToken token)
        {
            _tcpListener.Start();
            while (!token.IsCancellationRequested)
            {
                var client = _tcpListener.AcceptTcpClient();

                Task.Factory.StartNew(() =>
                {
                    using (var echoClient = new EchoClient(client))
                        echoClient.Dispatch(token);
                }, token);
            }
        }

        public void Dispose()
        {
            _tcpListener.Stop();
        }
    }

    class EchoClient : IDisposable
    {
        private readonly TcpClient _tcpClient;

        public EchoClient(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
        }

        public void Dispatch(CancellationToken token)
        {
            using (var stream = _tcpClient.GetStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream) {AutoFlush = true})
                while (!token.IsCancellationRequested)
                    writer.WriteLine(reader.ReadLine());
        }

        public void Dispose()
        {
            _tcpClient.Close();
        }
    }
}
