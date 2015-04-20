using System;
using System.IO;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.ServiceModel.Channels;
using System.Threading;

namespace JetBlack.Examples.RxNetwork
{
    public static class NetworkExtensions
    {
        public static async void Listen(this TcpListener tcpListener, BufferManager bufferManager, int bufferSize, Action<ISubject<ManagedBuffer,ManagedBuffer>, CancellationToken> onClient, CancellationToken token)
        {
            tcpListener.Start();

            while (!token.IsCancellationRequested)
            {
                var client = await tcpListener.AcceptTcpClientAsync();
                onClient(client.GetStream().ToBytesSubject(bufferManager, bufferSize, token), token);
            }

            tcpListener.Stop();
        }

        public static ISubject<ManagedBuffer, ManagedBuffer> ToFrameSubject(this Stream stream, BufferManager bufferManager, Func<Exception, bool> isCompleted, CancellationToken token)
        {
            return
                stream.ToSubject<Stream, ManagedBuffer, ManagedBuffer>(
                    (s, t) => ManagedBuffer.ReadFrameAsync(s, bufferManager, t),
                    isCompleted,
                    buf => buf == null,
                    async (s, content) =>
                    {
                        await content.WriteFrameAsync(s, token);
                        content.Dispose();
                    },
                    _ => stream.Close(),
                    stream.Close);
        }

        public static ISubject<ManagedBuffer, ManagedBuffer> ToBytesSubject(this TcpClient client, BufferManager bufferManager, int bufferSize, CancellationToken token)
        {
            return client.GetStream().ToBytesSubject(bufferManager, bufferSize, token);
        }

        public static ISubject<ManagedBuffer, ManagedBuffer> ToBytesSubject(this Stream stream, BufferManager bufferManager, int bufferSize, CancellationToken token)
        {
            return
                stream.ToSubject<Stream, ManagedBuffer, ManagedBuffer>(
                    (s,t) => ManagedBuffer.ReadAsync(s, bufferManager, bufferSize, t),
                    IsSocketClosed,
                    buf => buf == null,
                    async (s, content) =>
                    {
                        await content.WriteAsync(s, token);
                        content.Dispose();
                    },
                    _ => stream.Close(),
                    stream.Close);
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
