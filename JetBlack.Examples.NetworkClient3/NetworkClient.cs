using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JetBlack.Examples.NetworkClient3
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

            var subject = SubjectEx.Create<Stream, byte[], byte[]>(_tcpClient.GetStream(), async x => await ReadFrameAsync(x), IsSocketClosed, x => x == null, async (s, x) => await WriteFrameAsync(s, x), error => { }, () => { });

            subject.Subscribe(buf => Console.WriteLine(Encoding.UTF8.GetString(buf)));

            for (var line = Console.ReadLine(); !string.IsNullOrEmpty(line); line = Console.ReadLine())
                subject.OnNext(Encoding.UTF8.GetBytes(line));

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

        public static async Task<byte[]> ReadFrameAsyncOld(Stream source)
        {
            var buf = await ReadBytesAsync(source, new byte[4]);
            var length = BitConverter.ToInt32(buf, 0);
            buf = await ReadBytesAsync(source, new byte[length]);
            return buf;
        }

        public static async Task<byte[]> ReadFrameAsync(Stream source)
        {
            return await ReadBytesAsync(source, new byte[BitConverter.ToInt32(await ReadBytesAsync(source, new byte[4]), 0)]);
        }

        public static async Task<byte[]> ReadBytesAsync(Stream stream, byte[] buf)
        {
            var bytesRead = 0;
            while (bytesRead < buf.Length)
            {
                var count = buf.Length - bytesRead;
                var nbytes = await stream.ReadAsync(buf, bytesRead, count);
                if (nbytes == 0)
                    throw new EndOfStreamException();
                bytesRead += nbytes;
            }
            return buf;
        }

        public static async Task WriteFrameAsync(Stream stream, byte[] buf)
        {
            await WriteBytesAsync(stream, BitConverter.GetBytes(buf.Length));
            await WriteBytesAsync(stream, buf);
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
