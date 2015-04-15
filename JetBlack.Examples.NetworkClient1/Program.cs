using System;
using System.IO;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace JetBlack.Examples.NetworkClient1
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
            using (var writer = new StreamWriter(stream) { AutoFlush = true })
            {
                reader.ToObservable(x => x.ReadLineAsync(), string.IsNullOrEmpty).Subscribe(Console.WriteLine);
                var observer = writer.ToObserver<StreamWriter, string>(async (w, line) => await w.WriteLineAsync(line), IsWriteCompleted, error => { }, () => { });

                for (var line = Console.ReadLine(); !string.IsNullOrEmpty(line); line = Console.ReadLine())
                    observer.OnNext(line);
            }
            return cts.Token;
        }

        private static bool IsWriteCompleted(Exception error)
        {
            var ioException = error as IOException;
            var socketException = (ioException == null ? error : ioException.InnerException) as SocketException;
            return socketException != null && IsWriteCompleted(socketException.SocketErrorCode);
        }

        private static bool IsWriteCompleted(SocketError socketError)
        {
            return socketError == SocketError.ConnectionReset || socketError == SocketError.ConnectionAborted;
        }

        public void Dispose()
        {
            _tcpClient.Close();
        }
    }

}
