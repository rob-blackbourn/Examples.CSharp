using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace JetBlack.Examples.NetworkClient2
{
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

            //var subject = _tcpClient.ToSubject();
            var subject = _tcpClient.ToSubject(async reader => await reader.ReadLineAsync(), async (writer, line) => await writer.WriteLineAsync());

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