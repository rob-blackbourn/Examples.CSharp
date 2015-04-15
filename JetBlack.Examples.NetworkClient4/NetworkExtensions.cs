using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace JetBlack.Examples.NetworkClient4
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
                    error =>
                    {
                        client.Close();
                        observer.OnError(error);
                    },
                    () =>
                    {
                        client.Close();
                        observer.OnCompleted();
                    });

            subject.Subscribe(observer);
            observable.Subscribe(subject, token);

            return token;
        }

        public static async Task<byte[]> ReadBytesAsync(this Stream stream, byte[] buf, CancellationToken token)
        {
            var bytesRead = 0;
            while (bytesRead < buf.Length)
            {
                var count = buf.Length - bytesRead;
                var nbytes = await stream.ReadAsync(buf, bytesRead, count, token);
                if (nbytes == 0)
                    throw new EndOfStreamException();
                bytesRead += nbytes;
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
