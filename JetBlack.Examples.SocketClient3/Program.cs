using System;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JetBlack.Examples.SocketClient3
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
            var bufferManager = BufferManager.CreateBufferManager(2 << 16, 2 << 8);

            using (var client = new EchoClient())
            {
                client.Dispatch(splitArgs[0], int.Parse(splitArgs[1]), bufferManager, cts.Token).Wait(cts.Token);
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

        public async Task Dispatch(string hostname, int port, BufferManager bufferManager, CancellationToken token)
        {
            await _socket.ConnectAsync(hostname, port);

            var sender = _socket.ToFrameSender(SocketFlags.None, token);
            var receiver = _socket.ToFrameReceiver(SocketFlags.None, bufferManager);

            var disposable = receiver
                .SubscribeOn(TaskPoolScheduler.Default)
                .Subscribe(x =>
                {
                    Console.WriteLine("Read: " + Encoding.UTF8.GetString(x.Buffer, 0, x.Length));
                    x.Dispose();
                });

            for (var line = Console.ReadLine(); !token.IsCancellationRequested && !string.IsNullOrEmpty(line); line = Console.ReadLine())
            {
                var writeBuffer = Encoding.UTF8.GetBytes(line);
                sender.OnNext(new ManagedBuffer(writeBuffer, writeBuffer.Length, null));
            }

            disposable.Dispose();
        }

        public void Dispose()
        {
            _socket.Close();
        }
    }
}
