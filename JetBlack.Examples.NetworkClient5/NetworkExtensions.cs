using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace JetBlack.Examples.NetworkClient5
{
    public static class NetworkExtensions
    {
        public static async Task<CancellationToken> Dispatch(string hostname, int port, IObservable<byte[]> observable, IObserver<byte[]> observer, CancellationToken token)
        {
            var client = new TcpClient();
            await client.ConnectAsync(hostname, port);

            var stream = client.GetStream();

            var subject =
                stream.ToSubject<Stream, byte[], byte[]>(
                    ReadFrameAsync,
                    IsSocketClosed,
                    x => x == null,
                    async (s, b) => await WriteFrameAsync(s, b, token),
                    Invokeable.Chain<Exception>(_ => client.Close(), observer.OnError),
                    Invokeable.Chain(client.Close, observer.OnCompleted));

            subject.Subscribe(observer, token);
            observable.Subscribe(subject, token);

            return token;
        }

        public static async Task<byte[]> ReadBytesAsync(this Stream stream, byte[] buf, CancellationToken token)
        {
            var count = 0;
            while (count < buf.Length)
            {
                var bytesRead = await stream.ReadAsync(buf, count, buf.Length - count, token);
                if (bytesRead == 0)
                    throw new EndOfStreamException();

                count += bytesRead;
            }
            return buf;
        }

        public static async Task WriteBytesAsync(this Stream stream, byte[] buf, CancellationToken token)
        {
            await stream.WriteAsync(buf, 0, buf.Length, token);
        }

        public static bool IsSocketClosed(this Exception error)
        {
            var ioException = error as IOException;
            var socketException = (ioException == null ? error : ioException.InnerException) as SocketException;
            return socketException != null && IsSocketClosed(socketException.SocketErrorCode);
        }

        public static bool IsSocketClosed(this SocketError socketError)
        {
            return socketError == SocketError.ConnectionReset || socketError == SocketError.ConnectionAborted;
        }

        public static async Task<byte[]> ReadFrameAsync(Stream stream, CancellationToken token)
        {
            var length = BitConverter.ToInt32(await stream.ReadBytesAsync(new byte[4], token), 0);
            return await stream.ReadBytesAsync(new byte[length], token);
        }

        public static async Task WriteFrameAsync(Stream stream, byte[] buf, CancellationToken token)
        {
            await stream.WriteBytesAsync(BitConverter.GetBytes(buf.Length), token);
            await stream.WriteBytesAsync(buf, token);
        }
    }
}
