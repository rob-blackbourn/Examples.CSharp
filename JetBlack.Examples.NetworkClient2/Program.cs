using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace JetBlack.Examples.NetworkClient2
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
            var cts = new CancellationTokenSource();

            await _tcpClient.ConnectAsync(hostname, port);

            var stream = _tcpClient.GetStream();
            var subject = SubjectEx.Create<StreamReader, StreamWriter, string, string>(new StreamReader(stream), source => source.ReadLineAsync(), IsSocketClosed, string.IsNullOrEmpty, source => source.Dispose(), new StreamWriter(stream) { AutoFlush = true }, async (w, line) => await w.WriteLineAsync(line), IsSocketClosed, error => { }, () => { }, sink => sink.Dispose());

            subject.Subscribe(Console.WriteLine);
            for (var line = Console.ReadLine(); !string.IsNullOrEmpty(line); line = Console.ReadLine())
                subject.OnNext(line);

            return cts.Token;
        }

        private static bool IsSocketClosed(Exception error)
        {
            var ioException = error as IOException;
            var socketException = (ioException == null ? error : ioException.InnerException) as SocketException;
            return socketException != null && IsSocketClosed(socketException.SocketErrorCode);
        }

        private static bool IsSocketClosed(SocketError socketError)
        {
            return socketError == SocketError.ConnectionReset || socketError == SocketError.ConnectionAborted;
        }

        public void Dispose()
        {
            _tcpClient.Close();
        }
    }
}
