using System;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBlack.Examples.RxNetwork;

namespace JetBlack.Examples.RxNetworkClient
{
    class Program
    {
        private static void Main(string[] args)
        {
            string[] splitArgs = null;
            if (args.Length != 1 || (splitArgs = args[0].Split(new[] {':'}, StringSplitOptions.RemoveEmptyEntries)).Length != 2)
            {
                Console.WriteLine("usage: EchoClient <hostname>:<port>");
                Console.WriteLine("example:");
                Console.WriteLine("    > EchoClient localhost:9211");
                Environment.Exit(-1);
            }

            var hostname = splitArgs[0];
            var port = int.Parse(splitArgs[1]);

            const int maxFrameSize = 2 << 16;
            const int maxPendingFrames = 16;

            var cts = new CancellationTokenSource();
            var bufferManager = BufferManager.CreateBufferManager(maxPendingFrames * maxFrameSize, maxFrameSize);
            var client = new TcpClient(hostname, port);
            var subject = client.GetStream().ToFrameSubject(bufferManager, _ => false, cts.Token);

            subject.Subscribe(
                frameContent =>
                {
                    Console.WriteLine("OnNext: {0}", Encoding.UTF8.GetString(frameContent.Buffer, 0, frameContent.Length));
                    frameContent.Dispose();
                },
                error =>
                {
                    Console.WriteLine("OnError: {0}\r\n{1}", error.Message, error.StackTrace);
                    cts.Cancel();
                },
                () => Console.WriteLine("OnCompleted"), cts.Token);

            Task.Factory.StartNew(() =>
            {
                do
                {
                    var line = Console.In.ReadLine();
                    if (string.IsNullOrEmpty(line))
                    {
                        cts.Cancel();
                        break;
                    }

                    subject.OnNext(line.ToManagedBuffer(bufferManager));

                } while (!cts.Token.IsCancellationRequested);
            }, cts.Token);

            cts.Token.WaitHandle.WaitOne();
        }
    }
}
