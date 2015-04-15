using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace JetBlack.Examples.NetworkServer1
{
    class ServerNetworkClient : IDisposable
    {
        private readonly TcpClient _tcpClient;

        public ServerNetworkClient(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
        }

        public void Dispatch(CancellationToken token)
        {
            var subject = _tcpClient.GetStream().ToSubject(async x => await ReadBytesAsync(x), async (s, x) => await WriteBytesAsync(s, x), IsSocketClosed);
            subject.Subscribe(subject.OnNext);
            token.WaitHandle.WaitOne();
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