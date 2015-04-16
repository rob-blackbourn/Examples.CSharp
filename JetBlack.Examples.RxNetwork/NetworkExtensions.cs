using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace JetBlack.Examples.RxNetwork
{
    public static class NetworkExtensions
    {
        public static async void Listen(this TcpListener tcpListener, IScheduler scheduler, CancellationToken token)
        {
            tcpListener.Start();

            while (!token.IsCancellationRequested)
            {
                var client = await tcpListener.AcceptTcpClientAsync();
                var subject = client.ToBytesSubject(token);
                subject.SubscribeOn(scheduler).Subscribe(subject, token);
            }

            tcpListener.Stop();
        }

        public static ISubject<byte[], byte[]> ToTcpClientSubject(string hostname, int port, CancellationToken token)
        {
            var client = new TcpClient(hostname, port);
            return ToFrameSubject(client, _ => false, token);
        }

        public static async Task<ISubject<byte[], byte[]>> ToTcpClientSubjectAsync(string hostname, int port, CancellationToken token)
        {
            var client = new TcpClient();
            await client.ConnectAsync(hostname, port);
            return ToFrameSubject(client, _ => false, token);
        }

        public static ISubject<byte[], byte[]> ToFrameSubject(this TcpClient client, Func<Exception,bool> isCompleted, CancellationToken token)
        {
            return
                client.GetStream().ToSubject<Stream, byte[], byte[]>(
                    ReadFrameAsync,
                    isCompleted,
                    buf => buf == null,
                    async (stream, buf) => await WriteFrameAsync(stream, buf, token),
                    _ =>
                    {
                        Debug.Print("Here");
                        client.Close();
                    },
                    () =>
                    {
                        Debug.Print("Here");
                        client.Close();
                    });
        }

        public static ISubject<byte[], byte[]> ToBytesSubject(this TcpClient client, CancellationToken token)
        {
            return
                client.GetStream().ToSubject<Stream, byte[], byte[]>(
                    ReadBytesAvailableAsync,
                    IsSocketClosed,
                    buf => buf == null,
                    async (stream, buf) => await WriteBytesAsync(stream, buf, token),
                    _ =>
                    {
                        Debug.Print("Here");
                        client.Close();
                    },
                    () =>
                    {
                        Debug.Print("Here");
                        client.Close();
                    });
        }

        public static async Task<byte[]> ReadFrameAsync(Stream stream, CancellationToken token)
        {
            var length = BitConverter.ToInt32(await stream.ReadBytesCompletelyAsync(new byte[4], token), 0);
            return await stream.ReadBytesCompletelyAsync(new byte[length], token);
        }

        public static async Task WriteFrameAsync(Stream stream, byte[] buf, CancellationToken token)
        {
            await stream.WriteBytesAsync(BitConverter.GetBytes(buf.Length), token);
            await stream.WriteBytesAsync(buf, token);
        }

        public static async Task<byte[]> ReadBytesAvailableAsync(Stream source, CancellationToken token)
        {
            var inputBuffer = new byte[1024];
            var bytesRead = await source.ReadAsync(inputBuffer, 0, inputBuffer.Length, token);
            if (bytesRead == 0)
                return null;
            var buf = new byte[bytesRead];
            Array.Copy(inputBuffer, buf, bytesRead);
            return buf;
        }

        public static async Task<byte[]> ReadBytesCompletelyAsync(this Stream stream, byte[] buf, CancellationToken token)
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
    }
}
