using System;
using System.IO;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace JetBlack.Examples.NetworkServer2
{
    public static class NetworkExtensions
    {
        public static async void Listen(this TcpListener tcpListener, IScheduler scheduler, CancellationToken token)
        {
            tcpListener.Start();

            while (!token.IsCancellationRequested)
            {
                var client = await tcpListener.AcceptTcpClientAsync();
                var subject = client.ToSubject(token);
                subject.SubscribeOn(scheduler).Subscribe(subject, token);
            }

            tcpListener.Stop();
        }

        public static ISubject<byte[], byte[]> ToSubject(this TcpClient client, CancellationToken token)
        {
            return
                client.GetStream().ToSubject<Stream, byte[], byte[]>(
                    ReadBytesAsync,
                    IsSocketClosed,
                    buf => buf == null,
                    async (stream, buf) => await WriteBytesAsync(stream, buf, token),
                    _ => client.Close(),
                    client.Close);
        }

        public static bool IsSocketClosed(Exception error)
        {
            var ioException = error as IOException;
            var socketException = (ioException == null ? error : ioException.InnerException) as SocketException;
            return socketException != null && IsSocketClosed(socketException.SocketErrorCode);
        }

        public static bool IsSocketClosed(SocketError socketError)
        {
            return socketError == SocketError.ConnectionReset || socketError == SocketError.ConnectionAborted;
        }

        public static async Task<byte[]> ReadBytesAsync(Stream source, CancellationToken token)
        {
            var inputBuffer = new byte[1024];
            var bytesRead = await source.ReadAsync(inputBuffer, 0, inputBuffer.Length, token);
            if (bytesRead == 0)
                return null;
            var buf = new byte[bytesRead];
            Array.Copy(inputBuffer, buf, bytesRead);
            return buf;
        }

        public static async Task WriteBytesAsync(Stream stream, byte[] buf, CancellationToken token)
        {
            await stream.WriteAsync(buf, 0, buf.Length, token);
        }
    }
}
