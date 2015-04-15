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

            var subject = SubjectEx.Create<Stream, byte[], byte[]>(_tcpClient.GetStream(), async x => await ReadFrameAsync(x), NetworkExtensions.IsSocketClosed, x => x == null, async (s, x) => await WriteFrameAsync(s, x), error => { }, () => { });

            subject.Subscribe(buf => Console.WriteLine(Encoding.UTF8.GetString(buf)));

            for (var line = Console.ReadLine(); !string.IsNullOrEmpty(line); line = Console.ReadLine())
                subject.OnNext(Encoding.UTF8.GetBytes(line));

            return cts.Token;
        }

        public static async Task<byte[]> ReadFrameAsync(Stream stream)
        {
            var length = BitConverter.ToInt32(await stream.ReadBytesAsync(new byte[4]), 0);
            return await stream.ReadBytesAsync(new byte[length]);
        }

        public static async Task WriteFrameAsync(Stream stream, byte[] buf)
        {
            await stream.WriteBytesAsync(BitConverter.GetBytes(buf.Length));
            await stream.WriteBytesAsync(buf);
        }

        public void Dispose()
        {
            _tcpClient.Close();
        }
    }
}
