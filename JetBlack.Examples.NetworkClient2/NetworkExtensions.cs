using System;
using System.IO;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace JetBlack.Examples.NetworkClient2
{
    public static class NetworkExtensions
    {
        public static ISubject<string, string> ToSubject(this TcpClient tcpClient)
        {
            var stream = tcpClient.GetStream();
            return SubjectEx.Create<StreamReader, StreamWriter, string, string>(new StreamReader(stream), async source => await source.ReadLineAsync(), IsSocketClosed, string.IsNullOrEmpty, source => source.Dispose(), new StreamWriter(stream) { AutoFlush = true }, async (w, line) => await w.WriteLineAsync(line), IsSocketClosed, error => { }, () => { }, sink => sink.Dispose());
        }

        public static ISubject<string, string> ToSubject(this TcpClient tcpClient, Func<StreamReader, Task<string>> producer, Func<StreamWriter, string, Task> consumer)
        {
            var stream = tcpClient.GetStream();
            return SubjectEx.Create<StreamReader, StreamWriter, string, string>(new StreamReader(stream), producer, IsSocketClosed, string.IsNullOrEmpty, source => source.Dispose(), new StreamWriter(stream) { AutoFlush = true }, async (w, line) => await w.WriteLineAsync(line), IsSocketClosed, error => { }, () => { }, sink => sink.Dispose());
        }

        public static ISubject<byte[], byte[]> ToSubject2(this TcpClient tcpClient)
        {
            var stream = tcpClient.GetStream();
            return
                SubjectEx.Create<Stream, byte[], byte[]>(
                stream,
                async source =>
                {
                    var inputBuffer = new byte[1024];
                    var bytesRead = await source.ReadAsync(inputBuffer, 0, inputBuffer.Length);
                    if (bytesRead == 0)
                        return null;
                    var buf = new byte[bytesRead];
                    Array.Copy(inputBuffer, buf, bytesRead);
                    return buf;
                },
                    IsSocketClosed,
                    x => x == null,
                    async (s, buf) => await s.WriteAsync(buf, 0, buf.Length), IsSocketClosed,
                    error => { },
                    () => { },
                    x => x.Dispose());
        }

        public static ISubject<byte[], byte[]> ToSubject(this Stream stream)
        {
            return stream.ToSubject(async x => await ReadBytesAsync(x), async (s, x) => await WriteBytesAsync(s, x), IsSocketClosed);
        }

        public static ISubject<byte[], byte[]> ToSubject(this Stream stream, Func<Stream, Task<byte[]>> producer, Func<Stream, byte[], Task> consumer, Func<Exception,bool> isClosed)
        {
            return SubjectEx.Create<Stream, byte[], byte[]>(stream, async x => await producer(x), isClosed, x => x == null, async (s, buf) => await consumer(s, buf), error => { }, () => { });
        }

        public static async Task<byte[]> ReadBytesAsync(this Stream source)
        {
            var inputBuffer = new byte[1024];
            var bytesRead = await source.ReadAsync(inputBuffer, 0, inputBuffer.Length);
            if (bytesRead == 0)
                return null;
            var buf = new byte[bytesRead];
            Array.Copy(inputBuffer, buf, bytesRead);
            return buf;
        }

        public static async Task WriteBytesAsync(this Stream stream, byte[] buf)
        {
            await stream.WriteAsync(buf, 0, buf.Length);
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
    }
}
