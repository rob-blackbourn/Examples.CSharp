using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JetBlack.Examples.NetworkClient2
{
    class NetworkClient : IDisposable
    {
        private readonly TcpClient _tcpClient;

        public NetworkClient()
        {
            _tcpClient = new TcpClient();
        }

        public async Task<CancellationToken> Dispatch(string hostname, int port)
        {
            var cts = new CancellationTokenSource();

            await _tcpClient.ConnectAsync(hostname, port);

            var subject = _tcpClient.GetStream().ToSubject(async x => await ReadBytesAsync(x), async (s, x) => await WriteBytesAsync(s, x), IsSocketClosed);

            var sb = new StringBuilder();
            subject.Subscribe(buf =>
            {
                var s = Encoding.UTF8.GetString(buf);
                var index = s.IndexOf('\n');
                if (index == -1)
                    sb.Append(s);
                else
                {
                    sb.Append(s.Remove(index));
                    Console.WriteLine(sb);
                    sb.Clear();
                    sb.Append(s.Substring(index + 1));
                }
            });

            for (var line = Console.ReadLine(); !string.IsNullOrEmpty(line); line = Console.ReadLine())
                subject.OnNext(Encoding.UTF8.GetBytes(line + '\n'));

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

        public static async Task<byte[]> ReadBytesAsync(Stream source)
        {
            var inputBuffer = new byte[1024];
            var bytesRead = await source.ReadAsync(inputBuffer, 0, inputBuffer.Length);
            if (bytesRead == 0)
                return null;
            var buf = new byte[bytesRead];
            Array.Copy(inputBuffer, buf, bytesRead);
            return buf;
        }

        public static async Task WriteBytesAsync(Stream stream, byte[] buf)
        {
            await stream.WriteAsync(buf, 0, buf.Length);
        }
        public void Dispose()
        {
            _tcpClient.Close();
        }
    }
}
