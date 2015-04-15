using System;
using System.IO;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace JetBlack.Examples.NetworkClient3
{
    public static class NetworkExtensions
    {
        public static ISubject<byte[], byte[]> ToSubject(this Stream stream, Func<Stream, Task<byte[]>> producer, Func<Stream, byte[], Task> consumer, Func<Exception, bool> isClosed)
        {
            return SubjectEx.Create<Stream, byte[], byte[]>(stream, async x => await producer(x), isClosed, x => x == null, async (s, buf) => await consumer(s, buf), error => { }, () => { });
        }

        public static async Task<byte[]> ReadBytesAsync(this Stream stream, byte[] buf)
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

        public static async Task WriteBytesAsync(this Stream stream, byte[] buf)
        {
            await stream.WriteAsync(buf, 0, buf.Length);
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
