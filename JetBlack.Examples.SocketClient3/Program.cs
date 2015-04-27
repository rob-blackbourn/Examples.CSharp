using System;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;

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

            var host = splitArgs[0];
            var port = int.Parse(splitArgs[1]);

            var cts = new CancellationTokenSource();
            var bufferManager = BufferManager.CreateBufferManager(2 << 16, 2 << 8);

            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(host, port);

            var receiverDisposable = socket.ToFrameReceiver(SocketFlags.None, bufferManager)
                .SubscribeOn(TaskPoolScheduler.Default)
                .Subscribe(x =>
                {
                    Console.WriteLine("Read: " + Encoding.UTF8.GetString(x.Buffer, 0, x.Length));
                    x.Dispose();
                },
                error => Console.WriteLine("Error: " + error.Message));

            var sender = socket.ToFrameSender(SocketFlags.None, cts.Token);

            for (var line = Console.ReadLine(); !cts.IsCancellationRequested && !string.IsNullOrEmpty(line); line = Console.ReadLine())
            {
                var writeBuffer = Encoding.UTF8.GetBytes(line);
                sender.OnNext(new ManagedBuffer(writeBuffer, writeBuffer.Length, null));
            }

            receiverDisposable.Dispose();

            cts.Cancel();
        }
    }
}
